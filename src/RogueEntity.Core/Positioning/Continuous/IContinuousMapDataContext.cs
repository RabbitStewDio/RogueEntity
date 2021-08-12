namespace RogueEntity.Core.Positioning.Continuous
{
    public interface IContinuousMapDataContext<TItemId>
    {
        bool TryUpdateItemPosition(TItemId itemId, in ContinuousMapPosition desiredPosition);
        void MarkDirty<TPosition>(in TPosition position) where TPosition: IPosition<TPosition>;
    }
}