using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Positioning;

public class DefaultMapContext<TItemId> : IConfigurableMapContext<TItemId>, IMapContextInitializer<TItemId>
{
    readonly List<MapLayer> mapLayers;
    readonly Dictionary<byte, IMapDataContext<TItemId>> mapLayerData;

    public DefaultMapContext(DynamicDataViewConfiguration config)
    {
        this.Config = config;
        this.mapLayers = new List<MapLayer>();
        this.mapLayerData = new Dictionary<byte, IMapDataContext<TItemId>>();
    }

    public DynamicDataViewConfiguration Config { get; }

    public void ResetState()
    {
        foreach (var x in mapLayerData)
        {
            x.Value.ResetState();
        }
    }

    public IConfigurableMapContext<TItemId> WithMapLayer(MapLayer layer, IMapDataContext<TItemId> data)
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

    public ReadOnlyListWrapper<MapLayer> Layers()
    {
        return mapLayers;
    }

    public bool TryGetMapDataFor(MapLayer layer, out IMapDataContext<TItemId> data)
    {
        return TryGetMapDataFor(layer.LayerId, out data);
    }

    public bool TryGetMapDataFor(byte layerId, out IMapDataContext<TItemId> data)
    {
        return mapLayerData.TryGetValue(layerId, out data);
    }
}