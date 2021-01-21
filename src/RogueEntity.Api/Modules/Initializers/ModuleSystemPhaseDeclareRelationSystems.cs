using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Services;
using RogueEntity.Api.Utils;
using Serilog;

namespace RogueEntity.Api.Modules.Initializers
{
    public class ModuleSystemPhaseDeclareRelationSystems : IModuleEntityInitializationCallback
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleSystem>();
        readonly GlobalModuleEntityInformation entityInfo;
        readonly IServiceResolver serviceResolver;
        readonly ModuleInitializer moduleInitializer;
        ModuleBase currentModule;

        public ModuleSystemPhaseDeclareRelationSystems(in ModuleSystemPhaseInitModuleResult p,
                                                       IServiceResolver serviceResolver)
        {
            this.serviceResolver = serviceResolver;
            this.entityInfo = p.EntityInformation;
            this.moduleInitializer = p.ModuleInitializer;
        }


        public void InitializeModuleRelations(List<ModuleRecord> orderedModules)
        {
            foreach (var mod in orderedModules)
            {
                if (mod.InitializedRelations)
                {
                    continue;
                }

                try
                {
                    moduleInitializer.CurrentModuleId = mod.ModuleId;
                    mod.InitializedRelations = true;

                    foreach (var subject in moduleInitializer.EntityInitializers)
                    {
                        currentModule = mod.Module;
                        subject.callback(this);
                        currentModule = null;
                    }
                }
                finally
                {
                    moduleInitializer.CurrentModuleId = null;
                }
            }
        }

        void IModuleEntityInitializationCallback.PerformInitialization<TEntityId>(IModuleInitializationData<TEntityId> moduleContext)
        {
            if (!entityInfo.TryGetModuleEntityInformation<TEntityId>(out var mi))
            {
                return;
            }

            var moduleInitializerParams = new ModuleEntityInitializationParameter<TEntityId>(mi, serviceResolver, moduleContext);

            foreach (var relation in mi.Relations)
            {
                // check precondition - is there no relation data available? ... then skip
                if (!mi.TryQueryRelationTarget(relation, out var targetTypes))
                {
                    Logger.Debug("Skipping initialization for relation {Relation} as no entity uses its target role", relation);
                    continue;
                }

                foreach (var targetType in targetTypes)
                {
                    foreach (var roleInitializer in CollectRelationInitializers<TEntityId>(targetType, currentModule, mi, relation))
                    {
                        if (IsValidRelation(roleInitializer, mi, relation))
                        {
                            Logger.Debug("Invoking module initializer {SourceHint} for entity {Entity} with relation {EntityRelation}", 
                                         roleInitializer.SourceHint, typeof(TEntityId), relation);
                            roleInitializer.Initializer(in moduleInitializerParams, moduleInitializer, relation);
                        }
                        else
                        {
                            Logger.Debug("Skipping module initializer {SourceHint} for entity {Entity} with relation {EntityRelation}", 
                                         roleInitializer.SourceHint, typeof(TEntityId), relation);
                        }
                    }
                }
            }
        }

        public bool IsValidRelation<TEntityId>(ModuleEntityRelationInitializerInfo<TEntityId> r, IModuleEntityInformation mi, EntityRelation relation)
            where TEntityId : IEntityKey
        {
            if (r.Relation != relation)
            {
                return false;
            }

            foreach (var requiredRole in r.RequiredObjectRoles)
            {
                if (!mi.HasRole(relation.Object, requiredRole))
                {
                    return false;
                }
            }

            foreach (var requiredRelation in r.RequiredSubjectRoles)
            {
                if (!mi.HasRole(relation.Subject, requiredRelation))
                {
                    return false;
                }
            }

            return true;
        }

        List<ModuleEntityRelationInitializerInfo<TEntityId>> CollectRelationInitializers<TEntityId>(Type targetType,
                                                                                                                  ModuleBase module,
                                                                                                                  IModuleEntityInformation mi,
                                                                                                                  EntityRelation relation)
            where TEntityId : IEntityKey
        {
            var subjectType = typeof(TEntityId);
            var retval = new List<ModuleEntityRelationInitializerInfo<TEntityId>>();

            var methodInfos = module.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var m in methodInfos)
            {
                var attr = m.GetCustomAttribute<InitializerCollectorAttribute>();
                if (attr == null || attr.Type != InitializerCollectorType.Relations)
                {
                    continue;
                }

                if (!m.IsSameGenericFunction(new[] {subjectType, targetType},
                                             out var genericMethod, out var errorHint,
                                             typeof(IEnumerable<ModuleEntityRelationInitializerInfo<TEntityId>>),
                                             typeof(IServiceResolver), typeof(IModuleEntityInformation), typeof(EntityRelation)))
                {
                    if (string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(
                            $"Expected a generic method with signature 'IEnumerable<ModuleEntityRoleInitializerInfo<TEntityId, TRelationTargetId>> Declare<TEntityId>(IServiceResolver, IModuleInitializer, EntityRelation), but found {m} in module {module.Id}");
                    }

                    Logger.Information("Generic constraints on module {Module} with method {Method} do not match. {ErrorHint}", module.Id, m.Name, errorHint);
                    continue;
                }

                if (genericMethod.Invoke(module, new object[] {serviceResolver, mi, relation}) is IEnumerable<ModuleEntityRelationInitializerInfo<TEntityId>> list)
                {
                    retval.AddRange(list);
                }
            }

            foreach (var m in methodInfos)
            {
                var attr = m.GetCustomAttribute<EntityRelationInitializerAttribute>();
                if (attr == null)
                {
                    continue;
                }

                if (attr.RelationName != relation.Id)
                {
                    continue;
                }

                if (!m.IsSameGenericAction(new[] {subjectType, targetType},
                                           out var genericMethod, out var errorHint,
                                           typeof(ModuleEntityInitializationParameter<TEntityId>).MakeByRefType(), typeof(IModuleInitializer), typeof(EntityRelation)))
                {
                    if (string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(
                            $"Expected a generic method with signature 'void XXX<TEntityId, TRelationTargetId>(ModuleEntityInitializationParameter ByRef, IModuleInitializer, EntityRole), but found {m} in module {module.Id}");
                    }

                    Logger.Information("Generic constraints on module {Module} with method {Method} do not match. {ErrorHint}", module.Id, m.Name, errorHint);
                    continue;
                }

                Logger.Verbose("Invoking role initializer {Method}", genericMethod.Name);
                var initializer = (ModuleEntityRelationInitializerDelegate<TEntityId>)
                    Delegate.CreateDelegate(typeof(ModuleEntityRelationInitializerDelegate<TEntityId>), module, genericMethod);
                retval.Add(ModuleEntityRelationInitializerInfo.CreateFor(relation, initializer)
                                                              .WithRequiredSubjectRoles(attr.ConditionalSubjectRoles.Select(e => new EntityRole(e)).ToArray())
                                                              .WithRequiredTargetRoles(attr.ConditionalObjectRoles.Select(e => new EntityRole(e)).ToArray())
                );
            }

            if (retval.Count == 0)
            {
                Logger.Verbose("No relation initializers defined for {Relation} with subject {Subject} and {Target}", relation, subjectType, targetType);
            }
            return retval;
        }
    }
}