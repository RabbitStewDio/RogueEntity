namespace RogueEntity.Core.Movement.GoalFinding
{
    public interface IGoalFinderSource
    {
        IGoalFinderBuilder GetPathFinder();
    }
}