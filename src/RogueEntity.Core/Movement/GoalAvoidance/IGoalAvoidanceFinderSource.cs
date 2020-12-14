namespace RogueEntity.Core.Movement.GoalAvoidance
{
    public interface IGoalAvoidanceFinderSource
    {
        IGoalAvoidanceFinderBuilder GetGoalFinder();
    }
}
