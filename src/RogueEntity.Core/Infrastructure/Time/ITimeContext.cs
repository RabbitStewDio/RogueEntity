namespace RogueEntity.Core.Infrastructure.Time
{
    public interface ITimeContext
    {
        ITimeSource TimeSource { get; }
    }
}