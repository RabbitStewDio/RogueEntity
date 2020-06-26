namespace RogueEntity.Core.Infrastructure.GameLoops
{
    public interface ITimeContext
    {
        ITimeSource TimeSource { get; }
    }
}