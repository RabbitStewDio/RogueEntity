using System;
using System.Collections.Generic;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Positioning.Grid
{
    public class OnDemandGridMapDataContext<TGameContext, TItemId> : IGridMapDataContext<TGameContext, TItemId>, IGridMapRawDataContext<TItemId>
    {
        public event EventHandler<PositionDirtyEventArgs> PositionDirty;
        
        readonly MapLayer layer;
        readonly int tileWidth;
        readonly int tileHeight;
        readonly Dictionary<int, IDynamicDataView2D<TItemId>> mapDataByDepth;

        public OnDemandGridMapDataContext(MapLayer layer, int tileWidth, int tileHeight)
        {
            this.layer = layer;
            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
            mapDataByDepth = new Dictionary<int, IDynamicDataView2D<TItemId>>();
        }

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

            rdata = new DynamicDataView<TItemId>(tileWidth, tileHeight);
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

        public virtual void MarkDirty<TPosition>(in TPosition position)
            where TPosition : IPosition
        {
            PositionDirty?.Invoke(this, new PositionDirtyEventArgs(Position.From(position), layer));
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