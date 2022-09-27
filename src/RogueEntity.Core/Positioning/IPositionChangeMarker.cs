using EnTTSharp;

namespace RogueEntity.Core.Positioning
{
    public interface IPositionChangeMarker<TPosition>
        where TPosition : IPosition<TPosition>
    {
        Optional<TPosition> PreviousPosition { get; }
    }
}