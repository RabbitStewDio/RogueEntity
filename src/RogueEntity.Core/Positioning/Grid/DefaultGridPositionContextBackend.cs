using System;
using System.Collections.Generic;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Positioning.Grid
{
    public class DefaultGridPositionContextBackend<TGameContext, TItemId> : IGridMapContext<TGameContext, TItemId>
    {
        readonly Dictionary<MapLayer, IGridMapDataContext<TGameContext, TItemId>> mapLayerData;
        readonly List<MapLayer> mapLayers;

        public DefaultGridPositionContextBackend()
        {
            mapLayers = new List<MapLayer>();
            mapLayerData = new Dictionary<MapLayer, IGridMapDataContext<TGameContext, TItemId>>();
        }

        public DefaultGridPositionContextBackend<TGameContext, TItemId> WithMapLayer(MapLayer layer, IGridMapDataContext<TGameContext, TItemId> data)
        {
            if (mapLayerData.ContainsKey(layer))
            {
                throw new ArgumentException($"Layer {layer} has already been declared.");
            }

            mapLayers.Add(layer);
            mapLayerData[layer] = data;
            return this;
        }

        public ReadOnlyListWrapper<MapLayer> GridLayers()
        {
            return mapLayers;
        }

        public bool TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<TGameContext, TItemId> data)
        {
            return mapLayerData.TryGetValue(layer, out data);
        }
    }
}