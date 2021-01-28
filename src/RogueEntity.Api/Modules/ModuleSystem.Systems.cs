using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Services;

namespace RogueEntity.Api.Modules
{
    public partial class ModuleSystem
    {
        IGameLoopSystemInformation InitializeSystems(IModuleInitializationData moduleInitializer,
                                                                   GlobalModuleEntityInformation globalModuleEntityInformation)
        {
            var globalSystems = CollectGlobalSystems(moduleInitializer);
            InitializeGlobalSystems(globalModuleEntityInformation, globalSystems);

            foreach (var entity in moduleInitializer.EntityInitializers)
            {
                var handler = new EntitySystemRegistrationHandler(globalModuleEntityInformation, serviceResolver, registrations);
                entity.callback(handler);
            }

            return registrations;
        }

        static IEnumerable<IGlobalSystemDeclaration> CollectGlobalSystems(IModuleInitializationData moduleInitializer)
        {
            var globalSystems = new Dictionary<EntitySystemId, IGlobalSystemDeclaration>();
            foreach (var globalSystem in moduleInitializer.GlobalSystems)
            {
                if (globalSystems.TryGetValue(globalSystem.Id, out var entry))
                {
                    if (entry.InsertionOrder >= globalSystem.InsertionOrder)
                    {
                        continue;
                    }
                }

                globalSystems[globalSystem.Id] = globalSystem;
            }

            return globalSystems.Values;
        }

        void InitializeGlobalSystems(GlobalModuleEntityInformation globalModuleEntityInformation,
                                     IEnumerable<IGlobalSystemDeclaration> globalSystems)
        {
            var mip = new ModuleInitializationParameter(globalModuleEntityInformation, serviceResolver);
            foreach (var globalSystem in globalSystems.OrderBy(e => e.InsertionOrder))
            {
                try
                {
                    registrations.EnterContext(globalSystem);
                    globalSystem.SystemRegistration(mip, registrations);
                }
                finally
                {
                    registrations.LeaveContext();
                }
            }
        }

        class EntitySystemRegistrationHandler : IModuleEntityInitializationCallback
        {
            readonly GlobalModuleEntityInformation globalInformation;
            readonly IServiceResolver serviceResolver;
            readonly ModuleEntitySystemRegistrations registrations;

            public EntitySystemRegistrationHandler(GlobalModuleEntityInformation globalInformation,
                                                   IServiceResolver serviceResolver,
                                                   ModuleEntitySystemRegistrations registrations)
            {
                this.globalInformation = globalInformation;
                this.serviceResolver = serviceResolver;
                this.registrations = registrations;
            }

            public void PerformInitialization<TEntityId>(IModuleInitializationData<TEntityId> moduleContext)
                where TEntityId : IEntityKey
            {
                if (!globalInformation.TryGetModuleEntityInformation<TEntityId>(out var mi))
                {
                    Logger.Debug("Entity {EntityType} does not define any roles", typeof(TEntityId));
                    return;
                }

                var mip = new ModuleEntityInitializationParameter<TEntityId>(mi, serviceResolver, moduleContext);
                var ctx = serviceResolver.Resolve<IItemContextBackend<TEntityId>>();

                var sortedEntries = CollectEntitySystemDeclarations(moduleContext);
                foreach (var system in sortedEntries)
                {
                    try
                    {
                        registrations.EnterContext(system);
                        system.EntityRegistration?.Invoke(mip, ctx.EntityRegistry);
                    }
                    finally
                    {
                        registrations.LeaveContext();
                    }
                }

                foreach (var system in sortedEntries)
                {
                    try
                    {
                        registrations.EnterContext(system);
                        system.EntitySystemRegistration?.Invoke(mip, registrations, ctx.EntityRegistry);
                    }
                    finally
                    {
                        registrations.LeaveContext();
                    }
                }
            }

            static List<IEntitySystemDeclaration<TEntityId>>
                CollectEntitySystemDeclarations<TEntityId>(IModuleInitializationData<TEntityId> moduleContext)
                where TEntityId : IEntityKey
            {
                var entitySystems = new Dictionary<EntitySystemId, IEntitySystemDeclaration<TEntityId>>();
                foreach (var system in moduleContext.EntitySystems)
                {
                    if (entitySystems.TryGetValue(system.Id, out var entry))
                    {
                        if (entry.InsertionOrder >= system.InsertionOrder)
                        {
                            continue;
                        }
                    }

                    entitySystems[system.Id] = system;
                }

                return entitySystems.Values.OrderBy(e => e.InsertionOrder).ToList();
            }
        }
    }
}