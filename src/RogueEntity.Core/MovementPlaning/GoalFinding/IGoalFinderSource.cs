namespace RogueEntity.Core.MovementPlaning.GoalFinding
{
    public interface IGoalFinderSource
    {
        IGoalFinderBuilder GetGoalFinder();
    }
}