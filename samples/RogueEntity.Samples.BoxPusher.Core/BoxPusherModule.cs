using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Time;
using RogueEntity.Core.MapLoading.Builder;
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
            where TActorId : struct, IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();

            ctx.Register(BoxPusherPlayerEntities, 40_000, RegisterPlayerEntities);
            ctx.Register(BoxPusherInitializeWinSystem, 40_000, RegisterInitializeWinSystem);
            ctx.Register(BoxPusherFinalizeWinSystem, 40_010, RegisterFinalizeWinSystem);
        }

        [EntityRoleFinalizer("Role.BoxPusher.Box",
                             ConditionalRoles = new[] { "Role.Core.Position.GridPositioned" })]
        public void InitializeBox<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                           IModuleInitializer initializer,
                                           EntityRole r)
            where TItemId : struct, IEntityKey
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
            where TItemId : struct, IEntityKey
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
            where TActorId : struct, IEntityKey
            where TItemId : struct, IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(BoxPusherEnsureMoveSystem, 1000, EnsureMoveSystemExists);
        }

        void EnsureMoveSystemExists<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter, EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
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
            where TItemId : struct, IEntityKey
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
            where TItemId : struct, IEntityKey
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
            where TItemId : struct, IEntityKey
        {
            registry.RegisterFlag<BoxPusherBoxMarker>();
        }

        void RegisterTargetSpotEntities<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            registry.RegisterFlag<BoxPusherTargetFieldMarker>();
        }

        void RegisterPlayerEntities<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter, EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
        {
            registry.RegisterNonConstructable<BoxPusherPlayerProfile>();
        }

        void RegisterFinalizeWinSystem<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter, IGameLoopSystemRegistration context, EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
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
            where TActorId : struct, IEntityKey
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

    }
}
