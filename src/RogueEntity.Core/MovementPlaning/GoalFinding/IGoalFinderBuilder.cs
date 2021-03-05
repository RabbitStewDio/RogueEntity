using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.MovementPlaning.Goals;

namespace RogueEntity.Core.MovementPlaning.GoalFinding
{
    public interface IGoalFinderBuilder
    {
        IGoalFinderBuilder WithSearchRadius(float radius);

        IGoalFinderBuilder WithGoal<TGoal>()
            where TGoal : IGoal;

        public IGoalFinder Build(in AggregateMovementCostFactors movementProfile);
    }
}
