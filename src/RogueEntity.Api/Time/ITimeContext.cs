namespace RogueEntity.Api.Time
{
    public interface ITimeContext
    {
        ITimeSource TimeSource { get; }
    }
}