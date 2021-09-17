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
    public class ModuleSystemPhaseFinalizeRoleSystems : IModuleEntityInitializationCallback
    {
        static readonly ILogger Logger = SLog.ForContext<ModuleSystem>();
        readonly GlobalModuleEntityInformation entityInfo;
        readonly IServiceResolver serviceResolver;
        readonly ModuleInitializer moduleInitializer;
        IModule currentModule;

        public ModuleSystemPhaseFinalizeRoleSystems(in ModuleSystemPhaseInitModuleResult p,
                                                IServiceResolver serviceResolver)
        {
            this.serviceResolver = serviceResolver;
            this.entityInfo = p.EntityInformation;
            this.moduleInitializer = p.ModuleInitializer;
        }
        
        public void InitializeModuleRoles(List<ModuleRecord> orderedModules)
        {
            foreach (var mod in orderedModules)
            {
                if (mod.FinalizedRoles)
                {
                    continue;
                }

                try
                {
                    moduleInitializer.CurrentModuleId = mod.ModuleId;

                    mod.FinalizedRoles = true;

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
                Logger.Debug("No such entity");
                return;
            }

            var moduleInitializerParams = new ModuleEntityInitializationParameter<TEntityId>(mi, serviceResolver, moduleContext);
            
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

        List<ModuleEntityRoleInitializerInfo<TEntityId>> CollectRoleInitializers<TEntityId>(IModule module, IModuleEntityInformation mi, EntityRole role)
            where TEntityId : IEntityKey
        {
            var entityType = typeof(TEntityId);
            var retval = new List<ModuleEntityRoleInitializerInfo<TEntityId>>();

            var methodInfos = module.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var m in methodInfos)
            {
                var attr = m.GetCustomAttribute<FinalizerCollectorAttribute>();
                if (attr == null || attr.Type != InitializerCollectorType.Roles)
                {
                    continue;
                }

                if (!m.IsSameGenericFunction(new[] {entityType},
                                             out var genericMethod, out var errorHint,
                                             typeof(IEnumerable<ModuleEntityRoleInitializerInfo<TEntityId>>),
                                             typeof(IServiceResolver), typeof(IModuleEntityInformation), typeof(EntityRole)))
                {
                    if (string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(
                            $"Expected a generic method with signature 'IEnumerable<ModuleEntityRoleInitializerInfo<TEntityId>> DeclareInitializers<TEntityId>(IServiceResolver, IModuleInitializer, EntityRole), but found {m} in module {module.Id}");
                    }

                    Logger.Information("Generic constraints on module {Module} with method {Method} do not match. {ErrorHint}", module.Id, m.Name, errorHint);
                    continue;
                }

                if (genericMethod.Invoke(module, new object[] {serviceResolver, mi, role}) is IEnumerable<ModuleEntityRoleInitializerInfo<TEntityId>> list)
                {
                    retval.AddRange(list);
                }
            }

            foreach (var m in methodInfos)
            {
                var attr = m.GetCustomAttribute<EntityRoleFinalizerAttribute>();
                if (attr == null)
                {
                    continue;
                }

                if (attr.RoleName != role.Id)
                {
                    continue;
                }

                if (!m.IsSameGenericAction(new[] {entityType},
                                           out var genericMethod, out var errorHint,
                                           typeof(ModuleEntityInitializationParameter<TEntityId>).MakeByRefType(), 
                                           typeof(IModuleInitializer), 
                                           typeof(EntityRole)))
                {
                    if (string.IsNullOrEmpty(errorHint))
                    {
                        throw new ArgumentException(
                            $"Expected a generic method with signature 'void XXX<TEntityId>(ModuleInitializationParameter ByRef, IModuleInitializer, EntityRole), but found {m} in module {module.Id}");
                    }

                    Logger.Information("Generic constraints on module {Module} with method {Method} do not match. {ErrorHint}", module.Id, m.Name, errorHint);
                    continue;
                }

                Logger.Verbose("Invoking role initializer {Method}", genericMethod.Name);
                var initializer = (ModuleEntityRoleInitializerDelegate<TEntityId>)
                    Delegate.CreateDelegate(typeof(ModuleEntityRoleInitializerDelegate<TEntityId>), module, genericMethod);
                retval.Add(ModuleEntityRoleInitializerInfo.CreateFor(role, initializer, "<Reflect> " +module.GetType() + "#" + genericMethod.Name)
                                                          .WithRequiredRolesAnywhereInSystem(attr.WithAnyRoles.Select(e => new EntityRole(e)).ToArray())
                                                          .WithRequiredRoles(attr.ConditionalRoles.Select(e => new EntityRole(e)).ToArray())
                                                          .WithRequiredRelations(entityInfo.ResolveRelationsById(attr.ConditionalRelations))
                );
            }
            if (retval.Count == 0)
            {
                Logger.Verbose("Module {Module} does not define finalizers for {Role} with subject {Subject}", module.Id, role, entityType);
            }

            return retval;
        }
    }
}