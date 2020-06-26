using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Infrastructure.Positioning.Grid
{
    public interface IGridMapContext<TGameContext, TItemId>
    {
        ReadOnlyListWrapper<MapLayer> GridLayers();
        bool TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<TGameContext, TItemId> data);
    }
}