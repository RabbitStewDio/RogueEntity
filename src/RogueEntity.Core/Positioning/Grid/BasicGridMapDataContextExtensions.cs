using EnTTSharp.Entities;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Positioning.Grid;

public static class BasicGridMapDataContextExtensions
{
    public static IConfigurableMapContext<TItemId> WithBasicGridMapLayer<TItemId>(this IConfigurableMapContext<TItemId> map, MapLayer layer)
        where TItemId : struct, IEntityKey
    {
        return map.WithMapLayer(layer, new BasicGridMapDataContext<TItemId>(layer, map.Config));
    }

    public static IConfigurableMapContext<TItemId> WithBasicGridMapLayer<TItemId>(this IConfigurableMapContext<TItemId> map, MapLayer layer, DynamicDataViewConfiguration conf)
        where TItemId : struct, IEntityKey
    {
        return map.WithMapLayer(layer, new BasicGridMapDataContext<TItemId>(layer, conf));
    }

    public static IConfigurableMapContext<TItemId> WithBasicGridMapLayer<TItemId>(this IConfigurableMapContext<TItemId> map, MapLayer layer, int offsetX, int offsetY, int tileWidth, int tileHeight)
        where TItemId : struct, IEntityKey
    {
        var conf = new DynamicDataViewConfiguration(offsetX, offsetY, tileWidth, tileHeight);
        return map.WithMapLayer(layer, new BasicGridMapDataContext<TItemId>(layer, conf));
    }

    public static IConfigurableMapContext<TItemId> WithBasicGridMapLayer<TItemId>(this IConfigurableMapContext<TItemId> map, MapLayer layer, int tileWidth, int tileHeight)
        where TItemId : struct, IEntityKey
    {
        var conf = new DynamicDataViewConfiguration(0, 0, tileWidth, tileHeight);
        return map.WithMapLayer(layer, new BasicGridMapDataContext<TItemId>(layer, conf));
    }

}