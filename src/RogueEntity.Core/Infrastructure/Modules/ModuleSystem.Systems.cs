using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules.Helpers;
using RogueEntity.Core.Infrastructure.Services;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public partial class ModuleSystem<TGameContext>
    {
        IGameLoopSystemInformation<TGameContext> InitializeSystems(IModuleInitializationData<TGameContext> moduleInitializer,
                                                                   GlobalModuleEntityInformation<TGameContext> globalModuleEntityInformation)
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

        static IEnumerable<IGlobalSystemDeclaration<TGameContext>> CollectGlobalSystems(IModuleInitializationData<TGameContext> moduleInitializer)
        {
            var globalSystems = new Dictionary<EntitySystemId, IGlobalSystemDeclaration<TGameContext>>();
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

        void InitializeGlobalSystems(GlobalModuleEntityInformation<TGameContext> globalModuleEntityInformation,
                                     IEnumerable<IGlobalSystemDeclaration<TGameContext>> globalSystems)
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

        class EntitySystemRegistrationHandler : IModuleEntityInitializationCallback<TGameContext>
        {
            readonly GlobalModuleEntityInformation<TGameContext> globalInformation;
            readonly IServiceResolver serviceResolver;
            readonly ModuleEntitySystemRegistrations<TGameContext> registrations;

            public EntitySystemRegistrationHandler(GlobalModuleEntityInformation<TGameContext> globalInformation,
                                                   IServiceResolver serviceResolver,
                                                   ModuleEntitySystemRegistrations<TGameContext> registrations)
            {
                this.globalInformation = globalInformation;
                this.serviceResolver = serviceResolver;
                this.registrations = registrations;
            }

            public void PerformInitialization<TEntityId>(IModuleInitializationData<TGameContext, TEntityId> moduleContext)
                where TEntityId : IEntityKey
            {
                if (!globalInformation.TryGetModuleEntityInformation<TEntityId>(out var mi))
                {
                    Logger.Debug("Entity {EntityType} does not define any roles", typeof(TEntityId));
                    return;
                }

                var mip = new ModuleInitializationParameter(mi, serviceResolver);
                var ctx = serviceResolver.Resolve<IItemContextBackend<TGameContext, TEntityId>>();

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

            static List<IEntitySystemDeclaration<TGameContext, TEntityId>>
                CollectEntitySystemDeclarations<TEntityId>(IModuleInitializationData<TGameContext, TEntityId> moduleContext)
                where TEntityId : IEntityKey
            {
                var entitySystems = new Dictionary<EntitySystemId, IEntitySystemDeclaration<TGameContext, TEntityId>>();
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