namespace RogueEntity.Core.Positioning.Continuous
{
    public interface IContinuousMapDataContext<TGameContext, TItemId>
    {
        bool TryGetItemAt(ContinuousMapPosition position, out TItemId itemAtPosition);
        bool TryUpdateItemPosition(TItemId itemId, in ContinuousMapPosition desiredPosition);
        void MarkDirty<TPosition>(in TPosition position) where TPosition: IPosition;
    }
}