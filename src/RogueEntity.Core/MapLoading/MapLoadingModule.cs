using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Services;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.MapLoading
{
    public class MapLoadingModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.MapLoading";
        static readonly EntitySystemId MapLoadingCommandsSystemId = "System.Core.MapLoading.CommandSystem";
        static readonly EntitySystemId MapLoadingSystemId = "System.Core.MapLoading.LoaderSystem";
        static readonly EntitySystemId SpawnActorsSystemId = "System.Core.MapLoading.SpawnActors";
        static readonly EntitySystemId CollectSpawnPointsSystemId = "System.Core.MapLoading.CollectSpawnPoints";

        static readonly EntitySystemId RegisterMapLoadingEntitiesSystemId = "Entities.Core.MapLoading";

        public MapLoadingModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Map Loading";
            Description = "Provides base classes and behaviours for loading maps based on observer positions";
            IsFrameworkModule = true;

            DeclareDependency(ModuleDependency.Of(PlayerModule.ModuleId));
        }

        [EntityRoleInitializer("Role.Core.Player.PlayerObserver")]
        protected void InitializeMapLoadingSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                           IModuleInitializer initializer,
                                                           EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(RegisterMapLoadingEntitiesSystemId, 30_000, RegisterEntities);
            entityContext.Register(MapLoadingCommandsSystemId, 30_000, RegisterMapLoaderCommandsSystem);
            entityContext.Register(MapLoadingSystemId, 31_000, RegisterMapLoaderSystem);
        }

        [EntityRelationInitializerAttribute("Relation.Core.Player.PlayerToSpawnPoint")]
        protected void InitializeSpawnPlayerRelation<TActorId, TItemId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                                        IModuleInitializer initializer,
                                                                        EntityRelation r)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(SpawnActorsSystemId, 32_000, RegisterSpawnPlayers<TActorId, TItemId>);

            var spawnPointEntityContext = initializer.DeclareEntityContext<TItemId>();
            spawnPointEntityContext.Register(CollectSpawnPointsSystemId, 31_500, RegisterCollectSpawnPoints<TActorId, TItemId>);
        }

        void RegisterCollectSpawnPoints<TActorId, TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                           IGameLoopSystemRegistration context,
                                                           EntityRegistry<TItemId> registry)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            var system = GetOrCreateSpawnSystem<TActorId, TItemId>(initParameter.ServiceResolver);

            context.AddFixedStepHandlers(system.StartCollectSpawnLocations);
            context.AddLateVariableStepHandlers(system.StartCollectSpawnLocations);

            var s = registry.BuildSystem()
                            .WithoutContext()
                            .WithInputParameter<EntityGridPosition>()
                            .WithInputParameter<PlayerSpawnLocation>()
                            .CreateSystem(system.CollectSpawnLocations);
            context.AddFixedStepHandlerSystem(s);
        }

        void RegisterSpawnPlayers<TActorId, TItemId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                     IGameLoopSystemRegistration context,
                                                     EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            var system = GetOrCreateSpawnSystem<TActorId, TItemId>(initParameter.ServiceResolver);

            var spawnAction = registry.BuildSystem()
                                      .WithoutContext()
                                      .WithInputParameter<PlayerObserverTag>()
                                      .WithInputParameter<ChangeLevelCommand>()
                                      .CreateSystem(system.SpawnPlayer);
            context.AddFixedStepHandlerSystem(spawnAction);

            var placeAction = registry.BuildSystem()
                                      .WithoutContext()
                                      .WithInputParameter<PlayerObserverTag>()
                                      .WithInputParameter<ChangeLevelPositionCommand>()
                                      .CreateSystem(system.PlacePlayerAfterLevelChange);
            context.AddFixedStepHandlerSystem(placeAction);
        }

        void RegisterMapLoaderCommandsSystem<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter,
                                                        IGameLoopSystemRegistration context,
                                                        EntityRegistry<TEntityId> registry)
            where TEntityId : IEntityKey
        {
            if (!initParameter.ServiceResolver.TryResolve(out IMapRegionSystem system))
            {
                return;
            }

            var handleNewPlayersSystem = registry.BuildSystem()
                                                 .WithoutContext()
                                                 .WithInputParameter<PlayerObserverTag>()
                                                 .WithInputParameter<NewPlayerTag>()
                                                 .CreateSystem(system.RequestLoadLevelFromNewPlayer);

            context.AddFixedStepHandlerSystem(handleNewPlayersSystem);

            var handleChangeLevelSystem = registry.BuildSystem()
                                                  .WithoutContext()
                                                  .WithInputParameter<PlayerObserverTag>()
                                                  .WithInputParameter<ChangeLevelCommand>()
                                                  .CreateSystem(system.RequestLoadLevelFromChangeLevelCommand);
            context.AddFixedStepHandlerSystem(handleChangeLevelSystem);

            var handleChangePositionSystem = registry.BuildSystem()
                                                     .WithoutContext()
                                                     .WithInputParameter<PlayerObserverTag>()
                                                     .WithInputParameter<ChangeLevelPositionCommand>()
                                                     .CreateSystem(system.RequestLoadLevelFromChangePositionCommand);
            context.AddFixedStepHandlerSystem(handleChangePositionSystem);
        }

        void RegisterMapLoaderSystem<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter,
                                                IGameLoopSystemRegistration context,
                                                EntityRegistry<TEntityId> registry)
            where TEntityId : IEntityKey
        {
            if (!initParameter.ServiceResolver.TryResolve(out IMapRegionSystem system))
            {
                return;
            }

            context.AddLateVariableStepHandlers(system.LoadChunks);
        }

        void RegisterEntities<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                       EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<ChangeLevelCommand>();
            registry.RegisterNonConstructable<ChangeLevelPositionCommand>();
        }

        BasicPlayerSpawnSystem<TActorId, TItemId> GetOrCreateSpawnSystem<TActorId, TItemId>(IServiceResolver serviceResolver)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            if (serviceResolver.TryResolve(out BasicPlayerSpawnSystem<TActorId, TItemId> spawnSystem))
            {
                return spawnSystem;
            }

            var system = new BasicPlayerSpawnSystem<TActorId, TItemId>(serviceResolver.Resolve<IMapRegionLoaderService<int>>(),
                                                                       serviceResolver.Resolve<IItemPlacementService<TActorId>>(),
                                                                       serviceResolver.Resolve<IItemPlacementLocationService<TActorId>>(),
                                                                       serviceResolver.Resolve<IItemResolver<TActorId>>(),
                                                                       serviceResolver.ResolveOptional<IEntityRandomGeneratorSource>());
            serviceResolver.Store(system);
            return system;
        }
    }
}
