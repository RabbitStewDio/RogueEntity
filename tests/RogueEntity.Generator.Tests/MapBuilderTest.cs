using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;

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
            var itemMapContext = new DefaultMapContext<ItemReference>(DynamicDataViewConfiguration.Default16X16)
                                 .WithBasicGridMapLayer(FloorLayer)
                                 .WithBasicGridMapLayer(ItemLayer);
            var actorEntityContext = new ItemContextBackend<ActorReference>(new ActorReferenceMetaData());
            var actorMapContext = new DefaultMapContext<ActorReference>(DynamicDataViewConfiguration.Default16X16)
                .WithBasicGridMapLayer(ActorLayer);

            var itemPlacementContext = new ItemPlacementServiceContext<ItemReference>()
                                   .WithLayer(FloorLayer, 
                                              new ItemPlacementService<ItemReference>(itemEntityContext.ItemResolver, itemMapContext),
                                              new ItemPlacementLocationService<ItemReference>(itemEntityContext.ItemResolver, itemMapContext))
                                   .WithLayer(ItemLayer, 
                                              new ItemPlacementService<ItemReference>(itemEntityContext.ItemResolver, itemMapContext),
                                              new ItemPlacementLocationService<ItemReference>(itemEntityContext.ItemResolver, itemMapContext));
            var actorPlacementContext = new ItemPlacementServiceContext<ActorReference>()
                .WithLayer(ActorLayer, 
                           new ItemPlacementService<ActorReference>(actorEntityContext.ItemResolver, actorMapContext),
                           new ItemPlacementLocationService<ActorReference>(actorEntityContext.ItemResolver, actorMapContext));

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
