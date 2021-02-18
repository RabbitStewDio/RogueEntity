using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Generator.Tests
{
    [TestFixture]
    public class MapBuilderTest
    {
        static readonly MapLayer FloorLayer = new MapLayer(1, "Floor");
        static readonly MapLayer ItemLayer = new MapLayer(2, "Items");
        static readonly MapLayer ActorLayer = new MapLayer(3, "Actors");

        [Test]
        public void Construct()
        {
            var itemEntityContext = new ItemContextBackend<ItemReference>(new ItemReferenceMetaData());
            var itemMapContext = new DefaultGridPositionContextBackend<ItemReference>()
                                 .WithDefaultMapLayer(FloorLayer)
                                 .WithDefaultMapLayer(ItemLayer);
            var actorEntityContext = new ItemContextBackend<ActorReference>(new ActorReferenceMetaData());
            var actorMapContext = new DefaultGridPositionContextBackend<ActorReference>()
                .WithDefaultMapLayer(ActorLayer);

            var itemPlacementContext = new ItemPlacementServiceContext<ItemReference>()
                                   .WithLayer(FloorLayer, 
                                              new GridItemPlacementService<ItemReference>(itemEntityContext.ItemResolver, itemMapContext),
                                              new GridItemPlacementLocationService<ItemReference>(itemEntityContext.ItemResolver, itemMapContext))
                                   .WithLayer(ItemLayer, 
                                              new GridItemPlacementService<ItemReference>(itemEntityContext.ItemResolver, itemMapContext),
                                              new GridItemPlacementLocationService<ItemReference>(itemEntityContext.ItemResolver, itemMapContext));
            var actorPlacementContext = new ItemPlacementServiceContext<ActorReference>()
                .WithLayer(ActorLayer, 
                           new GridItemPlacementService<ActorReference>(actorEntityContext.ItemResolver, actorMapContext),
                           new GridItemPlacementLocationService<ActorReference>(actorEntityContext.ItemResolver, actorMapContext));

            var mb = new MapBuilder();
            mb.WithLayer(FloorLayer, itemEntityContext.ItemResolver, itemMapContext, itemPlacementContext);
            mb.WithLayer(ItemLayer, itemEntityContext.ItemResolver, itemMapContext, itemPlacementContext);
            mb.WithLayer(ActorLayer, actorEntityContext.ItemResolver, actorMapContext, actorPlacementContext);

            mb.Layers.Should().Equal(FloorLayer, ItemLayer, ActorLayer);
            mb.TryGetItemRegistry(FloorLayer, out var floorReg).Should().BeTrue();
            floorReg.Should().BeSameAs(itemEntityContext.ItemRegistry);
            mb.TryGetItemRegistry(ItemLayer, out var itemReg).Should().BeTrue();
            itemReg.Should().BeSameAs(itemEntityContext.ItemRegistry);
            mb.TryGetItemRegistry(ActorLayer, out var actorReg).Should().BeTrue();
            actorReg.Should().BeSameAs(actorEntityContext.ItemRegistry);
        }
    }
}
