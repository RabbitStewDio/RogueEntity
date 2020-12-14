using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.Goals;

namespace RogueEntity.Core.Movement.GoalFinding
{
    public interface IGoalFinderBuilder
    {
        IGoalFinderBuilder WithSearchRadius(float radius);

        IGoalFinderBuilder WithGoal<TGoal>()
            where TGoal : IGoal;

        public IGoalFinder Build(in PathfindingMovementCostFactors movementProfile);
    }
}
