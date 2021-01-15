using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Positioning.Continuous
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    public interface IContinuousMapDataContext<TItemId>
    {
        bool TryGetItemAt(ContinuousMapPosition position, out TItemId itemAtPosition);
        bool TryUpdateItemPosition(TItemId itemId, in ContinuousMapPosition desiredPosition);
        void MarkDirty<TPosition>(in TPosition position) where TPosition: IPosition<TPosition>;
    }
}