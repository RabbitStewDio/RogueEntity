using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Services;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.MapLoading.PlayerSpawning
{
    public class PlayerSpawningModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.PlayerSpawning";

        public static readonly EntityRole PlayerSpawnPointRole = new EntityRole("Role.Core.PlayerSpawning.PlayerSpawnPoint");
        public static readonly EntityRelation PlayerToSpawnPointRelation = new EntityRelation("Relation.Core.PlayerSpawning.PlayerToSpawnPoint", PlayerModule.PlayerRole, PlayerSpawnPointRole, true);

        static readonly EntitySystemId SpawnActorsSystemId = "System.Core.PlayerSpawning.SpawnNewActors";
        static readonly EntitySystemId PlaceActorsSystemId = "System.Core.PlayerSpawning.PlaceActors";
        static readonly EntitySystemId CollectSpawnPointsSystemId = "System.Core.PlayerSpawning.CollectSpawnPoints";

        static readonly EntitySystemId PlayerSpawnPointComponentsId = "Entities.Core.PlayerSpawning.PlayerSpawnPoint";

        public PlayerSpawningModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Player Spawning";
            Description = "Provides base classes and behaviours for spawning players into a map";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(PlayerModule.ModuleId),
                                ModuleDependency.Of(MapLoadingModule.ModuleId));

            RequireRole(PlayerSpawnPointRole);
            RequireRelation(PlayerToSpawnPointRelation).WithImpliedRole(MapLoadingModule.ControlLevelLoadingRole);
        }

        [EntityRoleInitializer("Role.Core.PlayerSpawning.PlayerSpawnPoint")]
        protected void InitializePlayerSpawnPointRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                               IModuleInitializer initializer,
                                                               EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(PlayerSpawnPointComponentsId, -20_000, RegisterPlayerSpawnPointComponents);
        }

        [EntityRelationInitializer("Relation.Core.PlayerSpawning.PlayerToSpawnPoint")]
        protected void InitializeSpawnPlayerRelation<TActorId, TItemId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                                        IModuleInitializer initializer,
                                                                        EntityRelation r)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(SpawnActorsSystemId, 45_000, RegisterSpawnNewPlayers<TActorId, TItemId>);
            entityContext.Register(PlaceActorsSystemId, 49_500, RegisterPlacePlayersAfterLevelLoading<TActorId, TItemId>);

            var spawnPointEntityContext = initializer.DeclareEntityContext<TItemId>();
            spawnPointEntityContext.Register(CollectSpawnPointsSystemId, 49_000, RegisterCollectSpawnPoints<TActorId, TItemId>);
        }

        void RegisterPlayerSpawnPointComponents<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                         EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<PlayerSpawnLocation>();
        }

        void RegisterSpawnNewPlayers<TActorId, TItemId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                     IGameLoopSystemRegistration context,
                                                     EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            var system = GetOrCreateSpawnSystem<TActorId, TItemId>(initParameter.ServiceResolver);

            var handleNewPlayersSystem = registry.BuildSystem()
                                                 .WithoutContext()
                                                 .WithInputParameter<PlayerTag>()
                                                 .WithInputParameter<NewPlayerTag>()
                                                 .CreateSystem(system.RequestLoadLevelFromNewPlayer);
            context.AddFixedStepHandlerSystem(handleNewPlayersSystem);
        }

        void RegisterCollectSpawnPoints<TActorId, TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                           IGameLoopSystemRegistration context,
                                                           EntityRegistry<TItemId> registry)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            var system = GetOrCreateSpawnSystem<TActorId, TItemId>(initParameter.ServiceResolver);

            context.AddFixedStepHandlers(system.StartCollectSpawnLocations);

            var s = registry.BuildSystem()
                            .WithoutContext()
                            .WithInputParameter<EntityGridPosition>()
                            .WithInputParameter<PlayerSpawnLocation>()
                            .CreateSystem(system.CollectSpawnLocations);
            context.AddFixedStepHandlerSystem(s);
        }

        void RegisterPlacePlayersAfterLevelLoading<TActorId, TItemId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                                      IGameLoopSystemRegistration context,
                                                                      EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            var system = GetOrCreateSpawnSystem<TActorId, TItemId>(initParameter.ServiceResolver);

            var spawnAction = registry.BuildSystem()
                                      .WithoutContext()
                                      .WithInputParameter<PlayerTag>()
                                      .WithInputParameter<ChangeLevelCommand>()
                                      .CreateSystem(system.SpawnPlayer);
            context.AddFixedStepHandlerSystem(spawnAction);

            var placeAction = registry.BuildSystem()
                                      .WithoutContext()
                                      .WithInputParameter<PlayerTag>()
                                      .WithInputParameter<ChangeLevelPositionCommand>()
                                      .CreateSystem(system.PlacePlayerAfterLevelChange);
            context.AddFixedStepHandlerSystem(placeAction);
        }


        FlatLevelPlayerSpawnSystem<TActorId, TItemId> GetOrCreateSpawnSystem<TActorId, TItemId>(IServiceResolver serviceResolver)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            if (serviceResolver.TryResolve(out FlatLevelPlayerSpawnSystem<TActorId, TItemId> spawnSystem))
            {
                return spawnSystem;
            }

            var system = new FlatLevelPlayerSpawnSystem<TActorId, TItemId>(serviceResolver.Resolve<IItemPlacementService<TActorId>>(),
                                                                           serviceResolver.Resolve<IItemPlacementLocationService<TActorId>>(),
                                                                           serviceResolver.Resolve<IItemResolver<TActorId>>(),
                                                                           serviceResolver.Resolve<IPlayerSpawnInformationSource>(),
                                                                           serviceResolver.ResolveToReference<IMapAvailabilityService>(),
                                                                           serviceResolver.ResolveOptional<IEntityRandomGeneratorSource>());
            serviceResolver.Store(system);
            return system;
        }
    }
}
