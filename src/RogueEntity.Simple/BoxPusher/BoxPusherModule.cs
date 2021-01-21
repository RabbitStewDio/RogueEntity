using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Services;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Generator;
using static RogueEntity.Core.Movement.CostModifier.MovementCostModifiers;

namespace RogueEntity.Simple.BoxPusher
{
    [Module("BoxPusher")]
    public class BoxPusherModule : ModuleBase
    {
        public BoxPusherModule()
        {
            Id = "Game.BoxPusher";
        }

        [ModuleInitializer]
        void InitializeModule(in ModuleInitializationParameter mip, IModuleInitializer initializer)
        {
            mip.ServiceResolver.ConfigureLightPhysics();

            mip.ServiceResolver.ConfigureEntityType(ItemReferenceMetaData.Instance);
            mip.ServiceResolver.GetOrCreateDefaultGridMapContext<ItemReference>()
               .WithDefaultMapLayer(BoxPusherMapLayers.Floor)
               .WithDefaultMapLayer(BoxPusherMapLayers.Items);

            var itemPlacementContext = new ItemPlacementServiceContext<ItemReference>()
                                       .WithLayer(BoxPusherMapLayers.Floor,
                                                  mip.ServiceResolver.GetOrCreateGridItemPlacementService<ItemReference>(),
                                                  mip.ServiceResolver.GetOrCreateGridItemPlacementLocationService<ItemReference>())
                                       .WithLayer(BoxPusherMapLayers.Items, 
                                                  mip.ServiceResolver.GetOrCreateGridItemPlacementService<ItemReference>(),
                                                  mip.ServiceResolver.GetOrCreateGridItemPlacementLocationService<ItemReference>());
            mip.ServiceResolver.Store<IItemPlacementServiceContext<ItemReference>>(itemPlacementContext);


            mip.ServiceResolver.ConfigureEntityType(ActorReferenceMetaData.Instance);
            mip.ServiceResolver.GetOrCreateDefaultGridMapContext<ActorReference>()
               .WithDefaultMapLayer(BoxPusherMapLayers.Actors);

            var actorPlacementContext = new ItemPlacementServiceContext<ActorReference>()
                .WithLayer(BoxPusherMapLayers.Actors, 
                           mip.ServiceResolver.GetOrCreateGridItemPlacementService<ActorReference>(),
                           mip.ServiceResolver.GetOrCreateGridItemPlacementLocationService<ActorReference>());
            mip.ServiceResolver.Store<IItemPlacementServiceContext<ActorReference>>(actorPlacementContext);
        }

        [ContentInitializer]
        void InitializeContent(in ModuleInitializationParameter mip, IModuleInitializer initializer)
        {
            var serviceResolver = mip.ServiceResolver;
            var ctx = initializer.DeclareContentContext<ItemReference>();
            ctx.Activate(ctx.CreateBulkEntityBuilder(serviceResolver)
                            .DefineWall()
                            .WithMovementCostModifier(Blocked<WalkingMovement>())
                            .Declaration);

            ctx.Activate(ctx.CreateBulkEntityBuilder(serviceResolver)
                            .DefineFloor()
                            .WithMovementCostModifier(For<WalkingMovement>(1))
                            .Declaration);

            ctx.Activate(ctx.CreateBulkEntityBuilder(serviceResolver)
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
                                                             .WithMovement()
                                                             .AsPointCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1)
                                                             .Declaration);

            mip.ServiceResolver.Store<IPlayerServiceConfiguration>(new PlayerServiceConfiguration(playerId));
        }
    }
}
