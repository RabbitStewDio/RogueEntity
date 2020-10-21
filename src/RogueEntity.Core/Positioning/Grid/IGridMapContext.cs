using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Positioning.Grid
{
    /// <summary>
    ///   There is a hard assumption that the map sizes across different map layers are consistent
    ///   across all maps.
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    /// <typeparam name="TItemId"></typeparam>
    public interface IGridMapContext<TGameContext, TItemId>
    {
        ReadOnlyListWrapper<MapLayer> GridLayers();
        bool TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<TGameContext, TItemId> data);
        bool TryGetGridRawDataFor(MapLayer layer, out IGridMapRawDataContext<TItemId> data);
    }
}