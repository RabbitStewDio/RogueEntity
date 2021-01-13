using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.MovementPlaning.GoalFinding;
using RogueEntity.Core.MovementPlaning.Goals;

namespace RogueEntity.Core.MovementPlaning.GoalAvoidance
{
    public interface IGoalAvoidanceFinderBuilder
    {
        IGoalFinderBuilder WithSearchRadius(float radius);

        IGoalFinderBuilder WithGoal<TGoal>()
            where TGoal : IGoal;

        public IGoalAvoidanceFinder Build(in PathfindingMovementCostFactors movementProfile);

    }
}
