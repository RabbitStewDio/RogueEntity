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
    public class ModuleSystemPhaseDeclareRelationSystems<TGameContext> : IModuleEntityInitializationCallback<TGameContext>
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleSystem<TGameContext>>();
        readonly GlobalModuleEntityInformation<TGameContext> entityInfo;
        readonly IServiceResolver serviceResolver;
        readonly ModuleInitializer<TGameContext> moduleInitializer;
        ModuleBase currentModule;

        public ModuleSystemPhaseDeclareRelationSystems(in ModuleSystemPhaseInitModuleResult<TGameContext> p,
                                                       IServiceResolver serviceResolver)
        {
            this.serviceResolver = serviceResolver;
            this.entityInfo = p.EntityInformation;
            this.moduleInitializer = p.ModuleInitializer;
        }


        public void InitializeModuleRelations(List<ModuleRecord<TGameContext>> orderedModules)
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

        void IModuleEntityInitializationCallback<TGameContext>.PerformInitialization<TEntityId>(IModuleInitializationData<TGameContext, TEntityId> moduleContext)
        {
            if (!entityInfo.TryGetModuleEntityInformation<TEntityId>(out var mi))
            {
                return;
            }

            var moduleInitializerParams = new ModuleEntityInitializationParameter<TGameContext, TEntityId>(mi, serviceResolver, moduleContext);

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
                            roleInitializer.Initializer(in moduleInitializerParams, moduleInitializer, relation);
                        }
                        else
                        {
                            
                        }
                    }
                }
            }
        }

        public bool IsValidRelation<TEntityId>(ModuleEntityRelationInitializerInfo<TGameContext, TEntityId> r, IModuleEntityInformation mi, EntityRelation relation)
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

        List<ModuleEntityRelationInitializerInfo<TGameContext, TEntityId>> CollectRelationInitializers<TEntityId>(Type targetType,
                                                                                                                  ModuleBase module,
                                                                                                                  IModuleEntityInformation mi,
                                                                                                                  EntityRelation relation)
            where TEntityId : IEntityKey
        {
            var subjectType = typeof(TEntityId);
            var retval = new List<ModuleEntityRelationInitializerInfo<TGameContext, TEntityId>>();

            var methodInfos = module.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var m in methodInfos)
            {
                var attr = m.GetCustomAttribute<InitializerCollectorAttribute>();
                if (attr == null || attr.Type != InitializerCollectorType.Relations)
                {
                    continue;
                }

                if (!m.IsSameGenericFunction(new[] {typeof(TGameContext), subjectType, targetType},
                                             out var genericMethod, out var errorHint,
                                             typeof(IEnumerable<ModuleEntityRelationInitializerInfo<TGameContext, TEntityId>>),
                                             typeof(IServiceResolver), typeof(IModuleEntityInformation), typeof(EntityRelation)))
                {
                    if (string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(
                            $"Expected a generic method with signature 'IEnumerable<ModuleEntityRoleInitializerInfo<TGameContext, TEntityId, TRelationTargetId>> Declare<TGameContext, TEntityId>(IServiceResolver, IModuleInitializer<TGameContext>, EntityRelation), but found {m} in module {module.Id}");
                    }

                    Logger.Information("Generic constraints on module {Module} with method {Method} do not match. {errorHint}", module.Id, m, errorHint);
                    continue;
                }

                if (genericMethod.Invoke(module, new object[] {serviceResolver, mi, relation}) is IEnumerable<ModuleEntityRelationInitializerInfo<TGameContext, TEntityId>> list)
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

                if (!m.IsSameGenericAction(new[] {typeof(TGameContext), subjectType, targetType},
                                           out var genericMethod, out var errorHint,
                                           typeof(ModuleEntityInitializationParameter<TGameContext, TEntityId>).MakeByRefType(), typeof(IModuleInitializer<TGameContext>), typeof(EntityRelation)))
                {
                    if (string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(
                            $"Expected a generic method with signature 'void XXX<TGameContext, TEntityId, TRelationTargetId>(ModuleEntityInitializationParameter ByRef, IModuleInitializer<TGameContext>, EntityRole), but found {m} in module {module.Id}");
                    }

                    Logger.Information("Generic constraints on module {Module} with method {Method} do not match. {errorHint}", module.Id, m, errorHint);
                    continue;
                }

                Logger.Verbose("Invoking role initializer {Method}", genericMethod);
                var initializer = (ModuleEntityRelationInitializerDelegate<TGameContext, TEntityId>)
                    Delegate.CreateDelegate(typeof(ModuleEntityRelationInitializerDelegate<TGameContext, TEntityId>), module, genericMethod);
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