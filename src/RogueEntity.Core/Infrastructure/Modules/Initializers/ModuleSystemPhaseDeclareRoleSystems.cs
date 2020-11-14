using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Helpers;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Modules.Initializers
{
    public class ModuleSystemPhaseDeclareRoleSystems<TGameContext> : IModuleEntityInitializationCallback<TGameContext>
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleSystem<TGameContext>>();
        readonly GlobalModuleEntityInformation<TGameContext> entityInfo;
        readonly IServiceResolver serviceResolver;
        readonly ModuleInitializer<TGameContext> moduleInitializer;
        ModuleBase currentModule;

        public ModuleSystemPhaseDeclareRoleSystems(in ModuleSystemPhaseInitModuleResult<TGameContext> p,
                                                   IServiceResolver serviceResolver)
        {
            this.serviceResolver = serviceResolver;
            this.entityInfo = p.EntityInformation;
            this.moduleInitializer = p.ModuleInitializer;
        }
        
        public void InitializeModuleRoles(List<ModuleRecord<TGameContext>> orderedModules)
        {
            foreach (var mod in orderedModules)
            {
                if (mod.InitializedRoles)
                {
                    continue;
                }

                try
                {
                    moduleInitializer.CurrentModuleId = mod.ModuleId;

                    mod.InitializedRoles = true;

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
                Logger.Debug("No such entity");
                return;
            }

            var moduleInitializerParams = new ModuleEntityInitializationParameter<TGameContext, TEntityId>(mi, serviceResolver, null);
            
            foreach (var role in mi.Roles)
            {
                foreach (var roleInitializer in CollectRoleInitializers<TEntityId>(currentModule, mi, role))
                {
                    if (entityInfo.IsValidRole(roleInitializer, role))
                    {
                        Logger.Debug("Invoking module initializer {SourceHint} for entity {Entity} with role {EntityRole}", 
                                     roleInitializer.SourceHint, typeof(TEntityId), role);
                        roleInitializer.Initializer(in moduleInitializerParams, moduleInitializer, role);
                    }
                    else
                    {
                        Logger.Debug("Skipping module initializer {SourceHint} for entity {Entity} with role {EntityRole}", 
                                     roleInitializer.SourceHint, typeof(TEntityId), role);
                    }
                }
            }
        }

        List<ModuleEntityRoleInitializerInfo<TGameContext, TEntityId>> CollectRoleInitializers<TEntityId>(ModuleBase module, IModuleEntityInformation mi, EntityRole role)
            where TEntityId : IEntityKey
        {
            var entityType = typeof(TEntityId);
            var retval = new List<ModuleEntityRoleInitializerInfo<TGameContext, TEntityId>>();

            var methodInfos = module.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var m in methodInfos)
            {
                var attr = m.GetCustomAttribute<InitializerCollectorAttribute>();
                if (attr == null || attr.Type != InitializerCollectorType.Roles)
                {
                    continue;
                }

                if (!m.IsSameGenericFunction(new[] {typeof(TGameContext), entityType},
                                             out var genericMethod, out var errorHint,
                                             typeof(IEnumerable<ModuleEntityRoleInitializerInfo<TGameContext, TEntityId>>),
                                             typeof(IServiceResolver), typeof(IModuleEntityInformation), typeof(EntityRole)))
                {
                    if (string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(
                            $"Expected a generic method with signature 'IEnumerable<ModuleEntityRoleInitializerInfo<TGameContext, TEntityId>> DeclareInitializers<TGameContext, TEntityId>(IServiceResolver, IModuleInitializer<TGameContext>, EntityRole), but found {m} in module {module.Id}");
                    }

                    Logger.Information("Generic constraints on module {Module} with method {Method} do not match. {errorHint}", module.Id, m, errorHint);
                    continue;
                }

                if (genericMethod.Invoke(module, new object[] {serviceResolver, mi, role}) is IEnumerable<ModuleEntityRoleInitializerInfo<TGameContext, TEntityId>> list)
                {
                    retval.AddRange(list);
                }
            }

            foreach (var m in methodInfos)
            {
                var attr = m.GetCustomAttribute<EntityRoleInitializerAttribute>();
                if (attr == null)
                {
                    continue;
                }

                if (attr.RoleName != role.Id)
                {
                    continue;
                }

                if (!m.IsSameGenericAction(new[] {typeof(TGameContext), entityType},
                                           out var genericMethod, out var errorHint,
                                           typeof(ModuleEntityInitializationParameter<TGameContext, TEntityId>).MakeByRefType(), typeof(IModuleInitializer<TGameContext>), typeof(EntityRole)))
                {
                    if (string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(
                            $"Expected a generic method with signature 'void XXX<TGameContext, TEntityId>(ModuleInitializationParameter ByRef, IModuleInitializer<TGameContext>, EntityRole), but found {m} in module {module.Id}");
                    }

                    Logger.Information("Generic constraints on module {Module} with method {Method} do not match. {errorHint}", module.Id, m, errorHint);
                    continue;
                }

                Logger.Verbose("Invoking role initializer {Method}", genericMethod);
                var initializer = (ModuleEntityRoleInitializerDelegate<TGameContext, TEntityId>)
                    Delegate.CreateDelegate(typeof(ModuleEntityRoleInitializerDelegate<TGameContext, TEntityId>), module, genericMethod);
                retval.Add(ModuleEntityRoleInitializerInfo.CreateFor(role, initializer, "<Reflect> " +module.GetType() + "#" + genericMethod.Name)
                                                          .WithRequiredRolesAnywhereInSystem(attr.WithAnyRoles.Select(e => new EntityRole(e)).ToArray())
                                                          .WithRequiredRoles(attr.ConditionalRoles.Select(e => new EntityRole(e)).ToArray())
                                                          .WithRequiredRelations(entityInfo.ResolveRelationsById(attr.ConditionalRelations))
                );
            }
            if (retval.Count == 0)
            {
                Logger.Verbose("No role initializers defined for {Role} with subject {Subject}", role, entityType);
            }

            return retval;
        }
    }
}