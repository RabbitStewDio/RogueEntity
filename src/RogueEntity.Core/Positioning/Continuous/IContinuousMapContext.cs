using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Positioning.Continuous
{
    public interface IContinuousMapContext<TGameContext, TItemId>
    {
        ReadOnlyListWrapper<MapLayer> ContinuousLayers();
        bool TryGetContinuousDataFor(MapLayer layer, out IContinuousMapDataContext<TGameContext, TItemId> data);
    }
}