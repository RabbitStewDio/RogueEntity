﻿using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Tests.Positioning.Grid
{
    public class TestGridPositionContext : IGridMapContext<ItemReference>
                                           //, IItemContext<TestGridPositionContext, ItemReference>
    {
        readonly DefaultGridPositionContextBackend<ItemReference> mapBackend;

        public TestGridPositionContext()
        {
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

        public bool TryGetGridDataFor(byte layerId, out IGridMapDataContext<ItemReference> data)
        {
            return mapBackend.TryGetGridDataFor(layerId, out data);
        }

        public bool TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<ItemReference> data)
        {
            return mapBackend.TryGetGridDataFor(layer, out data);
        }
    }
}