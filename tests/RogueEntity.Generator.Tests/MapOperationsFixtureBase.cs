using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Generator.Tests.Fixtures;

namespace RogueEntity.Generator.Tests
{
    public abstract class MapOperationsFixtureBase : WhenFixtureSupport
    {
        protected static readonly MapLayer FloorLayer = new MapLayer(1, "Floor");
        protected static readonly MapLayer ItemLayer = new MapLayer(2, "Items");
        protected static readonly MapLayer ActorLayer = new MapLayer(3, "Actors");

        protected static readonly ItemDeclarationId BulkFloor1 = "Bulk.Floor.1";
        protected static readonly ItemDeclarationId BulkFloor2 = "Bulk.Floor.2";
        protected static readonly ItemDeclarationId BulkFloor3 = "Bulk.Floor.3";
        protected static readonly ItemDeclarationId BulkItem1 = "Bulk.Item.1";
        protected static readonly ItemDeclarationId BulkItem2 = "Bulk.Item.2";
        protected static readonly ItemDeclarationId BulkItem3 = "Bulk.Item.3";
        protected static readonly ItemDeclarationId ReferenceItem1 = "Reference.Item.1";
        protected static readonly ItemDeclarationId Actor = "Actor";

        protected ItemContextBackend<ItemReference> ItemEntityContext;
        protected DefaultGridPositionContextBackend<ItemReference> ItemMapContext;
        protected ItemPlacementServiceContext<ItemReference> ItemPlacementContext;
        protected ItemContextBackend<ActorReference> ActorEntityContext;
        protected DefaultGridPositionContextBackend<ActorReference> ActorMapContext;
        protected ItemPlacementServiceContext<ActorReference> ActorPlacementContext;
        protected MapBuilder MapBuilder;
        protected ItemFixture<ItemReference> Items;
        protected ItemFixture<ActorReference> Actors;

        [SetUp]
        public void SetUp()
        {
            ItemMapContext = new DefaultGridPositionContextBackend<ItemReference>()
                             .WithDefaultMapLayer(FloorLayer)
                             .WithDefaultMapLayer(ItemLayer);
            ItemEntityContext = new ItemContextBackend<ItemReference>(new ItemReferenceMetaData());
            ItemEntityContext.EntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<ItemReference>>();
            ItemEntityContext.EntityRegistry.Register<DestroyedMarker>();
            ItemEntityContext.EntityRegistry.Register<CascadingDestroyedMarker>();
            ItemEntityContext.EntityRegistry.RegisterNonConstructable<EntityGridPosition>();
            ItemEntityContext.EntityRegistry.RegisterNonConstructable<EntityGridPositionChangedMarker>();
            
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkFloor1)
                                                    .WithTrait(new BulkItemGridPositionTrait<ItemReference>(FloorLayer))
                                                    .WithTrait(new StackingBulkTrait<ItemReference>(5, 5))
            );
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkFloor2)
                                                        .WithTrait(new BulkItemGridPositionTrait<ItemReference>(FloorLayer))
            );
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkFloor3)
                                                        .WithTrait(new BulkItemGridPositionTrait<ItemReference>(FloorLayer))
            );
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkItem1)
                                                    .WithTrait(new BulkItemGridPositionTrait<ItemReference>(ItemLayer))
                                                    .WithTrait(new StackingBulkTrait<ItemReference>(5, 5))
            );
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkItem2)
                                                        .WithTrait(new BulkItemGridPositionTrait<ItemReference>(ItemLayer))
            );
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkItem3)
                                                        .WithTrait(new BulkItemGridPositionTrait<ItemReference>(ItemLayer))
            );
            ItemEntityContext.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>(ReferenceItem1)
                                                        .WithTrait(new ReferenceItemGridPositionTrait<ItemReference>(ItemLayer))
            );

            ItemPlacementContext = new ItemPlacementServiceContext<ItemReference>()
                                   .WithLayer(FloorLayer,
                                              new GridItemPlacementService<ItemReference>(ItemEntityContext.ItemResolver, ItemMapContext),
                                              new GridItemPlacementLocationService<ItemReference>(ItemEntityContext.ItemResolver, ItemMapContext))
                                   .WithLayer(ItemLayer,
                                              new GridItemPlacementService<ItemReference>(ItemEntityContext.ItemResolver, ItemMapContext),
                                              new GridItemPlacementLocationService<ItemReference>(ItemEntityContext.ItemResolver, ItemMapContext));
            Items = new ItemFixture<ItemReference>(ItemEntityContext.ItemResolver, ItemMapContext);

            ActorMapContext = new DefaultGridPositionContextBackend<ActorReference>().WithDefaultMapLayer(ActorLayer);
            ActorEntityContext = new ItemContextBackend<ActorReference>(new ActorReferenceMetaData());
            ActorEntityContext.EntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<ActorReference>>();
            ActorEntityContext.EntityRegistry.Register<DestroyedMarker>();
            ActorEntityContext.EntityRegistry.Register<CascadingDestroyedMarker>();
            ActorEntityContext.EntityRegistry.RegisterNonConstructable<EntityGridPosition>();
            ActorEntityContext.EntityRegistry.RegisterNonConstructable<EntityGridPositionChangedMarker>();
            ActorEntityContext.ItemRegistry.Register(new ReferenceItemDeclaration<ActorReference>(Actor)
                                                         .WithTrait(new ReferenceItemGridPositionTrait<ActorReference>(ActorLayer))
            );
            Actors = new ItemFixture<ActorReference>(ActorEntityContext.ItemResolver, ActorMapContext);

            ActorPlacementContext = new ItemPlacementServiceContext<ActorReference>()
                .WithLayer(ActorLayer,
                           new GridItemPlacementService<ActorReference>(ActorEntityContext.ItemResolver, ActorMapContext),
                           new GridItemPlacementLocationService<ActorReference>(ActorEntityContext.ItemResolver, ActorMapContext));

            MapBuilder = new MapBuilder();
            MapBuilder.WithLayer(FloorLayer, ItemEntityContext.ItemResolver, ItemMapContext, ItemPlacementContext);
            MapBuilder.WithLayer(ItemLayer, ItemEntityContext.ItemResolver, ItemMapContext, ItemPlacementContext);
            MapBuilder.WithLayer(ActorLayer, ActorEntityContext.ItemResolver, ActorMapContext, ActorPlacementContext);
        }

        public EntityContext<ItemFixture<ItemReference>, ItemReference> GivenAnItem(ItemDeclarationId item)
            => new EntityContext<ItemFixture<ItemReference>, ItemReference>(Items, item);

        public EntityContext<ItemFixture<ActorReference>, ActorReference> GivenAnActor(ItemDeclarationId item)
            => new EntityContext<ItemFixture<ActorReference>, ActorReference>(Actors, item);

        public EntityContext<ItemFixture<ItemReference>, ItemReference> GivenAnEmptyItem()
            => new EntityContext<ItemFixture<ItemReference>, ItemReference>(Items);

        public EntityContext<ItemFixture<ActorReference>, ActorReference> GivenAnEmptyActor()
            => new EntityContext<ItemFixture<ActorReference>, ActorReference>(Actors);
    }
}
