using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Chunks
{
    public class PooledGridPositionContextBackend<TEntityId>: IGridMapContext<TEntityId>
    {
        readonly List<MapLayer> mapLayers;
        readonly IBoundedDataViewPool<TEntityId> pool;
        readonly Dictionary<byte, PooledGridMapDataContext<TEntityId>> mapLayerData;
        
        public PooledGridPositionContextBackend(IBoundedDataViewPool<TEntityId> pool)
        {
            this.mapLayers = new List<MapLayer>();
            this.pool = pool;
            this.mapLayerData = new Dictionary<byte, PooledGridMapDataContext<TEntityId>>();
        }

        public ReadOnlyListWrapper<MapLayer> GridLayers()
        {
            return mapLayers;
        }

        public bool TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<TEntityId> data)
        {
            throw new NotImplementedException();
        }

        public bool TryGetGridDataFor(byte layerId, out IGridMapDataContext<TEntityId> data)
        {
            throw new NotImplementedException();
        }
        
        public PooledGridPositionContextBackend<TEntityId> WithMapLayer(MapLayer layer, PooledGridMapDataContext<TEntityId> data)
        {
            if (layer == MapLayer.Indeterminate)
            {
                throw new ArgumentException();
            }
            
            if (mapLayerData.ContainsKey(layer.LayerId))
            {
                throw new ArgumentException($"Layer {layer} has already been declared.");
            }

            mapLayers.Add(layer);
            mapLayerData[layer.LayerId] = data;
            return this;
        }

        public PooledGridPositionContextBackend<TEntityId> WithDefaultMapLayer(MapLayer layer)
        {
            return WithMapLayer(layer, new PooledGridMapDataContext<TEntityId>(layer, pool));
        }

        public PooledGridPositionContextBackend<TEntityId> WithDefaultMapLayer(MapLayer layer, IBoundedDataViewPool<TEntityId> pool)
        {
            return WithMapLayer(layer, new PooledGridMapDataContext<TEntityId>(layer, pool));
        }
    }
}
