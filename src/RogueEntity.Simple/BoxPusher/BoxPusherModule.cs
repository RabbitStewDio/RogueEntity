using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core;
using RogueEntity.Core.Chunks;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Generator;
using RogueEntity.Generator.MapFragments;
using RogueEntity.Simple.BoxPusher.ItemTraits;
using static RogueEntity.Core.Movement.CostModifier.MovementCostModifiers;

namespace RogueEntity.Simple.BoxPusher
{
    [Module("BoxPusher")]
    public class BoxPusherModule : ModuleBase
    {
        static readonly EntityRole BoxRole = new EntityRole("Role.BoxPusher.Box");
        static readonly EntityRole TargetSpotRole = new EntityRole("Role.BoxPusher.TargetSpot");

        static readonly EntityRelation BoxOccupiesTargetRelation = new EntityRelation("Relation.BoxPusher.BoxOccupiesTargetSpot", BoxRole, TargetSpotRole);
        static readonly EntityRelation PlayerToBoxRelation = new EntityRelation("Relation.BoxPusher.PlayerCountingBoxes", PlayerModule.PlayerRole, BoxRole);

        static readonly EntitySystemId BoxPusherBoxEntities = new EntitySystemId("Entities.BoxPusher.Box");
        static readonly EntitySystemId BoxPusherTargetSpotEntities = new EntitySystemId("Entities.BoxPusher.TargetSpot");

        static readonly EntitySystemId BoxPusherPlayerEntities = new EntitySystemId("Entities.BoxPusher.Player");
        static readonly EntitySystemId BoxPusherInitializeWinSystem = new EntitySystemId("System.BoxPusher.WinSystem.Initialize");
        static readonly EntitySystemId BoxPusherCollectTargetSpotsSystem = new EntitySystemId("System.BoxPusher.WinSystem.CollectTargetSpots");
        static readonly EntitySystemId BoxPusherCollectBoxesSystem = new EntitySystemId("System.BoxPusher.WinSystem.CollectBoxes");
        static readonly EntitySystemId BoxPusherFinalizeWinSystem = new EntitySystemId("System.BoxPusher.WinSystem.Finalize");

        public BoxPusherModule()
        {
            Id = "Game.BoxPusher";

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(PlayerModule.ModuleId),
                                ModuleDependency.Of(ChunkManagerModule.ModuleId));

            DeclareRelation<ActorReference, ItemReference>(PlayerToBoxRelation);
            DeclareRelation<ItemReference, ItemReference>(BoxOccupiesTargetRelation);
        }

        [ModuleInitializer]
        void InitializeModule(in ModuleInitializationParameter mip, IModuleInitializer initializer)
        {
            mip.ServiceResolver.ConfigureLightPhysics();
            mip.ServiceResolver.ConfigureEntity(ItemReferenceMetaData.Instance, BoxPusherMapLayers.Floor, BoxPusherMapLayers.Items);
            mip.ServiceResolver.ConfigureEntity(ActorReferenceMetaData.Instance, BoxPusherMapLayers.Actors);

            var mapBuilder = new MapBuilder().WithLayer<ItemReference>(BoxPusherMapLayers.Floor, mip.ServiceResolver)
                                             .WithLayer<ItemReference>(BoxPusherMapLayers.Items, mip.ServiceResolver)
                                             .WithLayer<ActorReference>(BoxPusherMapLayers.Actors, mip.ServiceResolver);
            var fragmentParser = new MapFragmentParser();
            var mapLoader = new BoxPusherMapLevelDataSource(mapBuilder, fragmentParser, mip.ServiceResolver.Resolve<IEntityRandomGeneratorSource>());
            mip.ServiceResolver.Store<IMapLevelDataSource<int>>(mapLoader);
            mip.ServiceResolver.Store<IMapLevelDataSourceSystem>(mapLoader);

            /*
            var profileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RogueEntity/BoxPusher");
            var profileDataRepoFact = new FileDataRepositoryFactory(profileDir, MessagePackSerializerOptions.Standard)
                                      .WithKey(new GuidValueConverter())
                                      .WithKey(new StringValueConverter());
            
            var profileManager = new DefaultPlayerProfileManager<BoxPusherPlayerProfile>(profileDataRepoFact.Create<Guid, BoxPusherPlayerProfile>("profiles"));
            */
            var profileManager = new InMemoryPlayerProfileManager<BoxPusherPlayerProfile>();
            profileManager.TryCreatePlayer(new BoxPusherPlayerProfile("Duffy Duck"), out _, out _);
            profileManager.TryCreatePlayer(new BoxPusherPlayerProfile("Bugs Bunny").RecordLevelComplete(1), out _, out _);
            mip.ServiceResolver.Store<IPlayerProfileManager<BoxPusherPlayerProfile>>(profileManager);

            mip.ServiceResolver.Store(new BoxPusherWinConditionSystems());
            mip.ServiceResolver.Store(BoxPusherLevelSystem<ActorReference, ItemReference>.Create(mip.ServiceResolver, BoxPusherMapLayers.Actors));
            mip.ServiceResolver.Store<IPlayerManager<ActorReference, BoxPusherPlayerProfile>>(
                new InMemoryPlayerManager<ActorReference, BoxPusherPlayerProfile>(
                    mip.ServiceResolver.Resolve<IItemResolver<ActorReference>>(),
                    mip.ServiceResolver.ResolveToReference<IPlayerServiceConfiguration>(),
                    profileManager));
        }

        [ContentInitializer]
        void InitializeContent(in ModuleInitializationParameter mip, IModuleInitializer initializer)
        {
            var serviceResolver = mip.ServiceResolver;
            var ctx = initializer.DeclareContentContext<ItemReference>();
            ctx.Activate(ctx.CreateBulkEntityBuilder(serviceResolver)
                            .DefineWall()
                            .Declaration);

            ctx.Activate(ctx.CreateBulkEntityBuilder(serviceResolver)
                            .DefineFloor()
                            .Declaration);

            ctx.Activate(ctx.CreateReferenceEntityBuilder(serviceResolver)
                            .DefineFloorTargetZone()
                            .WithMovementCostModifier(For<WalkingMovement>(1))
                            .Declaration);

            ctx.Activate(ctx.CreateReferenceEntityBuilder(serviceResolver)
                            .DefineBox()
                            .WithMovementCostModifier(Blocked<WalkingMovement>())
                            .Declaration);

            var actorContext = initializer.DeclareContentContext<ActorReference>();
            var playerId = actorContext.Activate(actorContext.CreateReferenceEntityBuilder(serviceResolver)
                                                             .DefinePlayer<ActorReference, ItemReference>()
                                                             .Declaration);

            mip.ServiceResolver.Store<IPlayerServiceConfiguration>(new PlayerServiceConfiguration(playerId));
        }


        [EntityRoleInitializer("Role.Core.Player",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        public void InitializePlayer<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                               IModuleInitializer initializer,
                                               EntityRole r)
            where TActorId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(BoxPusherPlayerEntities, 40_000, RegisterPlayerEntities);
            ctx.Register(BoxPusherInitializeWinSystem, 40_000, RegisterInitializeWinSystem);
            ctx.Register(BoxPusherFinalizeWinSystem, 40_010, RegisterFinalizeWinSystem);
        }

        [EntityRoleFinalizer("Role.BoxPusher.Box",
                             ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
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
                             ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        public void InitializeTargetSpot<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                  IModuleInitializer initializer,
                                                  EntityRole r)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(BoxPusherTargetSpotEntities, 40_000, RegisterTargetSpotEntities);
            ctx.Register(BoxPusherCollectTargetSpotsSystem, 40_005, RegisterTargetSpotWinConditionSystem);
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
            context.AddFixedStepHandlers(action);
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
            context.AddFixedStepHandlers(action);
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
            context.AddFixedStepHandlers(action);
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
            context.AddFixedStepHandlers(action);
        }
    }
}
