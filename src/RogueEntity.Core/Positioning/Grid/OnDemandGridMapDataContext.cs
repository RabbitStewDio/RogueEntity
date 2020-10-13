using System;
using System.Collections.Generic;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Positioning.Grid
{
    public class OnDemandGridMapDataContext<TGameContext, TItemId> : IGridMapDataContext<TGameContext, TItemId>
    {
        public int Width { get; }
        public int Height { get; }
        public event EventHandler<PositionDirtyEventArgs> PositionDirty;
        
        readonly MapLayer layer;
        readonly Dictionary<int, IMapData<TItemId>> mapDataByDepth;

        public OnDemandGridMapDataContext(MapLayer layer, int width, int height)
        {
            this.layer = layer;
            Width = width;
            Height = height;
            mapDataByDepth = new Dictionary<int, IMapData<TItemId>>();
        }

        public bool TryGetMap(int z, out IMapData<TItemId> data, MapAccess accessMode = MapAccess.ReadOnly)
        {
            if (mapDataByDepth.TryGetValue(z, out data))
            {
                return true;
            }

            if (accessMode == MapAccess.ReadOnly)
            {
                return false;
            }

            data = new DenseMapData<TItemId>(Width, Height);
            mapDataByDepth[z] = data;
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