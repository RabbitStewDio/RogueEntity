using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Generator.Tests
{
    [TestFixture]
    public class MapOperationsTest
    {
        static readonly MapLayer FloorLayer = new MapLayer(1, "Floor");
        static readonly MapLayer ItemLayer = new MapLayer(2, "Items");
        static readonly MapLayer ActorLayer = new MapLayer(3, "Actors");

        static readonly ItemDeclarationId BulkFloor1 = "Bulk.Floor.1";
        static readonly ItemDeclarationId BulkFloor2 = "Bulk.Floor.2";
        static readonly ItemDeclarationId BulkFloor3 = "Bulk.Floor.3";
        static readonly ItemDeclarationId BulkItem1 = "Bulk.Item.1";
        static readonly ItemDeclarationId BulkItem2 = "Bulk.Item.2";
        static readonly ItemDeclarationId BulkItem3 = "Bulk.Item.3";
        static readonly ItemDeclarationId ReferenceItem1 = "Ref.Item.1";
        static readonly ItemDeclarationId Actor = "Actor";
        
        ItemContextBackend<ItemReference> itemEntityContext;
        DefaultGridPositionContextBackend<ItemReference> itemMapContext;
        ItemContextBackend<ActorReference> actorEntityContext;
        DefaultGridPositionContextBackend<ActorReference> actorMapContext;
        MapBuilder mapBuilder;
        ItemPlacementServiceContext<ItemReference> itemPlacementContext;
        ItemPlacementServiceContext<ActorReference> actorPlacementContext;

        [SetUp]
        public void SetUp()
        {
            itemMapContext = new DefaultGridPositionContextBackend<ItemReference>()
                             .WithDefaultMapLayer(FloorLayer)
                             .WithDefaultMapLayer(ItemLayer);
            itemEntityContext = new ItemContextBackend<ItemReference>(new ItemReferenceMetaData());
            itemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkFloor1)
                                                        .WithTrait(new BulkItemGridPositionTrait<ItemReference>(FloorLayer))
            );
            itemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkFloor2)
                                                        .WithTrait(new BulkItemGridPositionTrait<ItemReference>(FloorLayer))
            );
            itemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkFloor3)
                                                        .WithTrait(new BulkItemGridPositionTrait<ItemReference>(FloorLayer))
            );
            itemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkItem1)
                                                        .WithTrait(new BulkItemGridPositionTrait<ItemReference>(ItemLayer))
            );
            itemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkItem2)
                                                        .WithTrait(new BulkItemGridPositionTrait<ItemReference>(ItemLayer))
            );
            itemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkItem3)
                                                        .WithTrait(new BulkItemGridPositionTrait<ItemReference>(ItemLayer))
            );
            itemEntityContext.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>(ReferenceItem1)
                                                        .WithTrait(new ReferenceItemGridPositionTrait<ItemReference>(ItemLayer))
            );

            itemPlacementContext = new ItemPlacementServiceContext<ItemReference>()
                                   .WithLayer(FloorLayer, 
                                              new GridItemPlacementService<ItemReference>(itemEntityContext.ItemResolver, itemMapContext),
                                              new GridItemPlacementLocationService<ItemReference>(itemEntityContext.ItemResolver, itemMapContext))
                                   .WithLayer(ItemLayer, 
                                              new GridItemPlacementService<ItemReference>(itemEntityContext.ItemResolver, itemMapContext),
                                              new GridItemPlacementLocationService<ItemReference>(itemEntityContext.ItemResolver, itemMapContext));

            actorMapContext = new DefaultGridPositionContextBackend<ActorReference>().WithDefaultMapLayer(ActorLayer);
            actorEntityContext = new ItemContextBackend<ActorReference>(new ActorReferenceMetaData());
            actorEntityContext.ItemRegistry.Register(new ReferenceItemDeclaration<ActorReference>(Actor)
                                                         .WithTrait(new ReferenceItemGridPositionTrait<ActorReference>(ActorLayer))
            );

            actorPlacementContext = new ItemPlacementServiceContext<ActorReference>()
                                    .WithLayer(ActorLayer, 
                                               new GridItemPlacementService<ActorReference>(actorEntityContext.ItemResolver, actorMapContext),
                                               new GridItemPlacementLocationService<ActorReference>(actorEntityContext.ItemResolver, actorMapContext));
            
            mapBuilder = new MapBuilder();
            mapBuilder.WithLayer(FloorLayer, itemEntityContext.ItemResolver, itemMapContext, itemPlacementContext);
            mapBuilder.WithLayer(ItemLayer, itemEntityContext.ItemResolver, itemMapContext, itemPlacementContext);
            mapBuilder.WithLayer(ActorLayer, actorEntityContext.ItemResolver, actorMapContext, actorPlacementContext);
        }

        [Test]
        public void TestClear()
        {
            itemMapContext.TryGetGridDataFor(ItemLayer, out var data).Should().BeTrue();
            data.TryGetWritableView(0, out var view, DataViewCreateMode.CreateMissing).Should().BeTrue();
            var someItem = itemEntityContext.ItemResolver.Instantiate(BulkItem1);
            view[0, 0] = someItem; // just fake some content.
            view[0, 0].Should().Be(someItem);

            mapBuilder.Clear(Position.Of(ItemLayer, 0, 0));
            view[0, 0].Should().Be(new ItemReference());
        }
        
        [Test]
        public void TestPlacement()
        {
            itemMapContext.TryGetGridDataFor(ItemLayer, out var data).Should().BeTrue();
            data.TryGetWritableView(0, out var view, DataViewCreateMode.CreateMissing).Should().BeTrue();
            var someItem = itemEntityContext.ItemResolver.Instantiate(BulkItem1);
            view[0, 0] = someItem; // just fake some content.
            view[0, 0].Should().Be(someItem);

            mapBuilder.Clear(Position.Of(ItemLayer, 0, 0));
            view[0, 0].Should().Be(new ItemReference());
        }
    }
}
