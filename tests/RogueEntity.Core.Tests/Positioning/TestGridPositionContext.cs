using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Positioning
{
    public class TestGridPositionContext : IGridMapContext<TestGridPositionContext, ItemReference>,
                                           IItemContext<TestGridPositionContext, ItemReference>
    {
        readonly DefaultGridPositionContextBackend<TestGridPositionContext, ItemReference> mapBackend;
        readonly ItemContextBackend<TestGridPositionContext, ItemReference> itemContextBackend;

        public TestGridPositionContext()
        {
            itemContextBackend = new ItemContextBackend<TestGridPositionContext, ItemReference>(new ItemReferenceMetaData());
            mapBackend = new DefaultGridPositionContextBackend<TestGridPositionContext, ItemReference>();
        }

        public TestGridPositionContext WithMapLayer(MapLayer layer, IGridMapDataContext<TestGridPositionContext, ItemReference> data)
        {
            mapBackend.WithMapLayer(layer, data);
            return this;
        }

        public ReadOnlyListWrapper<MapLayer> GridLayers()
        {
            return mapBackend.GridLayers();
        }

        public bool TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<TestGridPositionContext, ItemReference> data)
        {
            return mapBackend.TryGetGridDataFor(layer, out data);
        }

        public bool TryGetGridRawDataFor(MapLayer layer, out IGridMapRawDataContext<ItemReference> data)
        {
            return mapBackend.TryGetGridRawDataFor(layer, out data);
        }

        public ItemRegistry<TestGridPositionContext, ItemReference> ItemRegistry
        {
            get { return itemContextBackend.ItemRegistry; }
        }

        public EntityRegistry<ItemReference> EntityRegistry
        {
            get { return itemContextBackend.EntityRegistry; }
        }

        public IItemResolver<TestGridPositionContext, ItemReference> ItemResolver
        {
            get { return itemContextBackend.ItemResolver; }
        }
    }
}