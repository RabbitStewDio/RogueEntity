using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning.Continuous
{
    public interface IContinuousMapContext<TItemId>
    {
        ReadOnlyListWrapper<MapLayer> ContinuousLayers();
        bool TryGetContinuousDataFor(MapLayer layer, out IContinuousMapDataContext<TItemId> data);
    }
}