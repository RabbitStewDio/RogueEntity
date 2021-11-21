using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Services;
using RogueEntity.Api.Time;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.MapLoading.FlatLevelMaps;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.GridMovement;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Generator;
using RogueEntity.Samples.BoxPusher.Core.Commands;
using RogueEntity.Samples.BoxPusher.Core.ItemTraits;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Samples.BoxPusher.Core
{
    [Module("BoxPusher")]
    public partial class BoxPusherModule : ModuleBase
    {
        public static readonly EntityRole BoxRole = new EntityRole("Role.BoxPusher.Box");
        public static readonly EntityRole TargetSpotRole = new EntityRole("Role.BoxPusher.TargetSpot");

        public static readonly EntityRelation BoxOccupiesTargetRelation = new EntityRelation("Relation.BoxPusher.BoxOccupiesTargetSpot", BoxRole, TargetSpotRole, true);
        public static readonly EntityRelation PlayerToBoxRelation = new EntityRelation("Relation.BoxPusher.PlayerCountingBoxes", PlayerModule.PlayerRole, BoxRole);

        static readonly EntitySystemId BoxPusherBoxEntities = new EntitySystemId("Entities.BoxPusher.Box");
        static readonly EntitySystemId BoxPusherTargetSpotEntities = new EntitySystemId("Entities.BoxPusher.TargetSpot");

        static readonly EntitySystemId BoxPusherPlayerEntities = new EntitySystemId("Entities.BoxPusher.Player");
        static readonly EntitySystemId BoxPusherInitializeWinSystem = new EntitySystemId("System.BoxPusher.WinSystem.Initialize");
        static readonly EntitySystemId BoxPusherCollectTargetSpotsSystem = new EntitySystemId("System.BoxPusher.WinSystem.CollectTargetSpots");
        static readonly EntitySystemId BoxPusherCollectBoxesSystem = new EntitySystemId("System.BoxPusher.WinSystem.CollectBoxes");
        static readonly EntitySystemId BoxPusherFinalizeWinSystem = new EntitySystemId("System.BoxPusher.WinSystem.Finalize");

        static readonly EntitySystemId BoxPusherResetLevelCommandSystem = new EntitySystemId("System.BoxPusher.ResetLevelCommandHandler");
        static readonly EntitySystemId BoxPusherClearMapBeforeSpawnSystem = new EntitySystemId("System.BoxPusher.ClearMapBeforeSpawnPlayer");
        static readonly EntitySystemId BoxPusherFinalizeReloadLevelSystem = new EntitySystemId("System.BoxPusher.FinalizeReloadLevel");
        static readonly EntitySystemId BoxPusherEvictOldLevelWhenLoadingNewLevelSystem = new EntitySystemId("System.BoxPusher.EvictOldLevelWhenLoadingNewLevel");

        static readonly EntitySystemId BoxPusherEnsureMoveSystem = new EntitySystemId("System.BoxPusher.MoveSystem.Create");

        public BoxPusherModule()
        {
            Id = "Game.BoxPusher";

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(PlayerModule.ModuleId),
                                ModuleDependency.Of(GridMovementModule.ModuleId),
                                ModuleDependency.Of(MapLoadingModule.ModuleId),
                                ModuleDependency.Of(MapBuilderModule.ModuleId),
                                ModuleDependency.Of(GeneratorModule.ModuleId));

            RequireRelation(PlayerToBoxRelation);
            RequireRelation(BoxOccupiesTargetRelation);
        }


        [EntityRoleInitializer("Role.Core.Player",
                               ConditionalRoles = new[] { "Role.Core.Position.GridPositioned" })]
        public void InitializePlayer<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                               IModuleInitializer initializer,
                                               EntityRole r)
            where TActorId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();

            ctx.Register(BoxPusherResetLevelCommandSystem, 20_000, RegisterResetLevelCommandHandler);

            ctx.Register(BoxPusherPlayerEntities, 40_000, RegisterPlayerEntities);
            ctx.Register(BoxPusherInitializeWinSystem, 40_000, RegisterInitializeWinSystem);
            ctx.Register(BoxPusherFinalizeWinSystem, 40_010, RegisterFinalizeWinSystem);
            
            ctx.Register(BoxPusherEvictOldLevelWhenLoadingNewLevelSystem, 45_400, RegisterEvictOldLevelWhenLoadingNewLevel);
            ctx.Register(BoxPusherClearMapBeforeSpawnSystem, 48_000, RegisterClearMapBeforeSpawnPlayers);
            ctx.Register(BoxPusherFinalizeReloadLevelSystem, 49_600, RegisterFinalizeReloadLevel);
        }

        [EntityRoleFinalizer("Role.BoxPusher.Box",
                             ConditionalRoles = new[] { "Role.Core.Position.GridPositioned" })]
        public void InitializeBox<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                           IModuleInitializer initializer,
                                           EntityRole r)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(BoxPusherBoxEntities, 40_000, RegisterBoxEntities);
            ctx.Register(BoxPusherCollectBoxesSystem, 40_005, RegisterBoxWinConditionSystem);
        }

        [EntityRoleFinalizer("Role.BoxPusher.TargetSpot",
                             ConditionalRoles = new[] { "Role.Core.Position.GridPositioned" })]
        public void InitializeTargetSpot<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                  IModuleInitializer initializer,
                                                  EntityRole r)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(BoxPusherTargetSpotEntities, 40_000, RegisterTargetSpotEntities);
            ctx.Register(BoxPusherCollectTargetSpotsSystem, 40_005, RegisterTargetSpotWinConditionSystem);
        }

        [EntityRelationInitializer("Relation.BoxPusher.PlayerCountingBoxes")]
        [SuppressMessage("ReSharper", "UnusedTypeParameter")]
        public void InitializePlayerBoxRelation<TActorId, TItemId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                                   IModuleInitializer initializer,
                                                                   EntityRelation r)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(BoxPusherEnsureMoveSystem, 1000, EnsureMoveSystemExists);
        }

        void EnsureMoveSystemExists<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter, EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var sr = initParameter.ServiceResolver;
            var ms = new PushMoveCommandSystem<ActorReference, ItemReference>(sr.ResolveToReference<ITimeSource>(),
                                                                              sr.Resolve<IItemResolver<ActorReference>>(),
                                                                              sr.Resolve<IMovementDataProvider>(),
                                                                              sr.Resolve<IItemPlacementService<ActorReference>>(),
                                                                              sr.Resolve<IItemResolver<ItemReference>>(),
                                                                              sr.Resolve<IItemPlacementService<ItemReference>>()
            );

            initParameter.ServiceResolver.Store(ms);
            initParameter.ServiceResolver.Store<GridMoveCommandSystem<ActorReference>>(ms);
        }

        void RegisterBoxWinConditionSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, IGameLoopSystemRegistration context, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var system = initParameter.ServiceResolver.Resolve<BoxPusherWinConditionSystems>();
            var action = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<EntityGridPosition>()
                                 .WithInputParameter<BoxPusherBoxMarker>()
                                 .CreateSystem(system.CollectBoxPositions);
            context.AddFixedStepHandlerSystem(action);
        }

        void RegisterTargetSpotWinConditionSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, IGameLoopSystemRegistration context, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var system = initParameter.ServiceResolver.Resolve<BoxPusherWinConditionSystems>();
            var action = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<EntityGridPosition>()
                                 .WithInputParameter<BoxPusherTargetFieldMarker>()
                                 .CreateSystem(system.CollectTargetSpots);
            context.AddFixedStepHandlerSystem(action);
        }

        void RegisterBoxEntities<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterFlag<BoxPusherBoxMarker>();
        }

        void RegisterTargetSpotEntities<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterFlag<BoxPusherTargetFieldMarker>();
        }

        void RegisterPlayerEntities<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter, EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            registry.RegisterNonConstructable<BoxPusherPlayerProfile>();
            registry.RegisterNonConstructable<ResetLevelCommand>();
        }

        void RegisterFinalizeWinSystem<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter, IGameLoopSystemRegistration context, EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var system = initParameter.ServiceResolver.Resolve<BoxPusherWinConditionSystems>();
            var action = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<EntityGridPosition>()
                                 .WithInputParameter<PlayerTag>()
                                 .WithInputParameter<BoxPusherPlayerProfile>()
                                 .CreateSystem(system.FinishEvaluateWinCondition);
            context.AddFixedStepHandlerSystem(action);
        }

        void RegisterInitializeWinSystem<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter, IGameLoopSystemRegistration context, EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var system = initParameter.ServiceResolver.Resolve<BoxPusherWinConditionSystems>();
            var action = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<EntityGridPosition>()
                                 .WithInputParameter<PlayerTag>()
                                 .WithInputParameter<BoxPusherPlayerProfile>()
                                 .CreateSystem(system.FindPlayer);

            context.AddFixedStepHandlers(system.StartCheckWinCondition);
            context.AddFixedStepHandlerSystem(action);
        }

        void RegisterResetLevelCommandHandler<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                        IGameLoopSystemRegistration context,
                                                        EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var sys = GetOrCreateResetLevelSystem<TActorId>(initParameter.ServiceResolver);
            var action = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<PlayerTag>()
                                 .WithInputParameter<ResetLevelCommand>()
                                 .WithOutputParameter<CommandInProgress>()
                                 .CreateSystem(sys.ProcessResetLevelCommand);
            context.AddFixedStepHandlerSystem(action);
        }


        void RegisterEvictOldLevelWhenLoadingNewLevel<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                                IGameLoopSystemRegistration context,
                                                                EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var sys = GetOrCreateResetLevelSystem<TActorId>(initParameter.ServiceResolver);
            var action = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<PlayerTag>()
                                 .WithInputParameter<ChangeLevelCommand>()
                                 .WithInputParameter<CommandInProgress>()
                                 .CreateSystem(sys.EvictOldLevelOnChangeLevelCommand);
            context.AddFixedStepHandlerSystem(action);
        }

        void RegisterClearMapBeforeSpawnPlayers<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                          IGameLoopSystemRegistration context,
                                                          EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var sys = GetOrCreateResetLevelSystem<TActorId>(initParameter.ServiceResolver);
            var action = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<PlayerTag>()
                                 .WithInputParameter<EvictLevelRequest>()
                                 .CreateSystem(sys.ResetMapDataBeforeSpawningPlayer);
            context.AddFixedStepHandlerSystem(action);
        }

        void RegisterFinalizeReloadLevel<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                   IGameLoopSystemRegistration context,
                                                   EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var sys = GetOrCreateResetLevelSystem<TActorId>(initParameter.ServiceResolver);
            var action = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<PlayerTag>()
                                 .WithInputParameter<ChangeLevelRequest>()
                                 .WithInputParameter<ResetLevelCommand>()
                                 .WithOutputParameter<CommandInProgress>()
                                 .CreateSystem(sys.FinalizeResetLevelCommand);
            context.AddFixedStepHandlerSystem(action);
        }

        ResetLevelSystem<TActorId> GetOrCreateResetLevelSystem<TActorId>(IServiceResolver sr)
            where TActorId : IEntityKey
        {
            if (sr.TryResolve(out ResetLevelSystem<TActorId> sys))
            {
                return sys;
            }

            sys = new ResetLevelSystem<TActorId>(sr.Resolve<IMapRegionTrackerService<int>>());
            sr.Store(sys);
            return sys;
        }
    }
}
