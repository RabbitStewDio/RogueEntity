using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.GoalFinding;
using RogueEntity.Core.Movement.Goals;

namespace RogueEntity.Core.Movement.GoalAvoidance
{
    public interface IGoalAvoidanceFinderBuilder
    {
        IGoalFinderBuilder WithSearchRadius(float radius);

        IGoalFinderBuilder WithGoal<TGoal>()
            where TGoal : IGoal;

        public IGoalAvoidanceFinder Build(in PathfindingMovementCostFactors movementProfile);

    }
}
