using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning.Grid
{
    /// <summary>
    ///   Map data is a four dimensional data structure. Map data is a 3D map (X,Y,Z coordinates)
    ///   split into separate data layers (usually floor, items, actors etc). 
    /// 
    ///   There is a hard assumption that the map sizes across different map layers are consistent
    ///   across all maps derived from a given map context.
    /// </summary>
    /// <typeparam name="TItemId"></typeparam>
    public interface IGridMapContext<TItemId>
    {
        ReadOnlyListWrapper<MapLayer> GridLayers();
        bool TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<TItemId> data);
        bool TryGetGridDataFor(byte layerId, out IGridMapDataContext<TItemId> data);
    }
}