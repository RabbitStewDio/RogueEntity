using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Positioning
{
    public class TestGridPositionContext : IGridMapContext<ItemReference>
                                           //, IItemContext<TestGridPositionContext, ItemReference>
    {
        readonly DefaultGridPositionContextBackend<ItemReference> mapBackend;
        readonly ItemContextBackend<TestGridPositionContext, ItemReference> itemContextBackend;

        public TestGridPositionContext()
        {
            itemContextBackend = new ItemContextBackend<TestGridPositionContext, ItemReference>(new ItemReferenceMetaData());
            mapBackend = new DefaultGridPositionContextBackend<ItemReference>();
        }

        public TestGridPositionContext WithMapLayer(MapLayer layer, IGridMapDataContext<ItemReference> data)
        {
            mapBackend.WithMapLayer(layer, data);
            return this;
        }

        public int OffsetX
        {
            get { return mapBackend.OffsetX; }
        }

        public int OffsetY
        {
            get { return mapBackend.OffsetY; }
        }

        public int TileSizeX
        {
            get { return mapBackend.TileSizeX; }
        }

        public int TileSizeY
        {
            get { return mapBackend.TileSizeY; }
        }

        public ReadOnlyListWrapper<MapLayer> GridLayers()
        {
            return mapBackend.GridLayers();
        }

        public bool TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<ItemReference> data)
        {
            return mapBackend.TryGetGridDataFor(layer, out data);
        }

        // public IItemRegistryBackend<TestGridPositionContext, ItemReference> ItemRegistry
        // {
        //     get { return itemContextBackend.ItemRegistry; }
        // }
        //
        // public EntityRegistry<ItemReference> EntityRegistry
        // {
        //     get { return itemContextBackend.EntityRegistry; }
        // }
        //
        // public IItemResolver<TestGridPositionContext, ItemReference> ItemResolver
        // {
        //     get { return itemContextBackend.ItemResolver; }
        // }
    }
}