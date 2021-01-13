namespace RogueEntity.Core.MovementPlaning.GoalAvoidance
{
    public interface IGoalAvoidanceFinderSource
    {
        IGoalAvoidanceFinderBuilder GetGoalFinder();
    }
}
