using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Positioning.Grid
{
    public sealed class DefaultGridMapDataContext<TGameContext, TItemId> : IGridMapDataContext<TGameContext, TItemId>, IGridMapRawDataContext<TItemId>
    {
        public event EventHandler<PositionDirtyEventArgs> PositionDirty;

        readonly int offsetX;
        readonly int offsetY;
        readonly int tileWidth;
        readonly int tileHeight;
        readonly Dictionary<int, IDynamicDataView2D<TItemId>> mapDataByDepth;

        public DefaultGridMapDataContext(MapLayer layer, int tileWidth, int tileHeight) : this(layer, 0, 0, tileWidth, tileHeight) { }

        public DefaultGridMapDataContext(MapLayer layer, int offsetX, int offsetY, int tileWidth, int tileHeight)
        {
            this.Layer = layer;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
            mapDataByDepth = new Dictionary<int, IDynamicDataView2D<TItemId>>();
        }
        
        [UsedImplicitly] 
        public MapLayer Layer { get; }

        public bool TryGetRaw(int z, out IDynamicDataView2D<TItemId> data, MapAccess accessMode = MapAccess.ReadOnly)
        {
            if (mapDataByDepth.TryGetValue(z, out var rdata))
            {
                data = rdata;
                return true;
            }

            if (accessMode == MapAccess.ReadOnly)
            {
                data = default;
                return false;
            }

            rdata = new DynamicDataView<TItemId>(offsetX, offsetY, tileWidth, tileHeight);
            mapDataByDepth[z] = rdata;
            data = rdata;
            return true;
        }

        public bool TryGetMap(int z, out IView2D<TItemId> data, MapAccess accessMode = MapAccess.ReadOnly)
        {
            if (mapDataByDepth.TryGetValue(z, out var rdata))
            {
                data = rdata;
                return true;
            }

            if (accessMode == MapAccess.ReadOnly)
            {
                data = default;
                return false;
            }

            rdata = new DynamicDataView<TItemId>(tileWidth, tileHeight);
            mapDataByDepth[z] = rdata;
            data = rdata;
            return true;
        }

        public void MarkDirty<TPosition>(in TPosition position)
            where TPosition : IPosition
        {
            PositionDirty?.Invoke(this, new PositionDirtyEventArgs(Position.From(position)));
        }

        public List<int> QueryActiveZLevels(List<int> cachedResults = null)
        {
            if (cachedResults == null)
            {
                cachedResults = new List<int>(mapDataByDepth.Count);
            }
            else
            {
                cachedResults.Clear();
            }

            foreach (var k in mapDataByDepth.Keys)
            {
                cachedResults.Add(k);
            }

            return cachedResults;
        }
    }
}