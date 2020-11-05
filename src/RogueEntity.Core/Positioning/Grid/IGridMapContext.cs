using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Positioning.Grid
{
    /// <summary>
    ///   There is a hard assumption that the map sizes across different map layers are consistent
    ///   across all maps derived from a given map context.
    /// </summary>
    /// <typeparam name="TItemId"></typeparam>
    public interface IGridMapContext<TItemId>: IGridMapConfiguration<TItemId>
    {
        ReadOnlyListWrapper<MapLayer> GridLayers();
        bool TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<TItemId> data);
    }

    public interface IGridMapConfiguration<TItemId>
    {
        int OffsetX { get; }
        int OffsetY { get; }
        int TileSizeX { get; }
        int TileSizeY { get; }
    }
}