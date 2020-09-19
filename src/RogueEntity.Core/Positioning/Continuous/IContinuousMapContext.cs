using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Positioning.Continuous
{
    public interface IContinuousMapContext<TGameContext, TItemId>
    {
        ReadOnlyListWrapper<MapLayer> ContinuousLayers();
        bool TryGetContinuousDataFor(MapLayer layer, out IContinuousMapDataContext<TGameContext, TItemId> data);
    }

    public interface IContinuousMapDataContext<TGameContext, TItemId>
    {
        bool TryGetItemAt(ContinuousMapPosition position, out TItemId itemAtPosition);
        bool TryUpdateItemPosition(TItemId itemId, in ContinuousMapPosition desiredPosition);
        void MarkDirty(in ContinuousMapPosition position);
    }
}