using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.Pathfinding;

namespace RogueEntity.Core.Movement.GoalFinding
{
    public interface IGoalFinderBuilder
    {
        IGoalFinderBuilder WithSearchRadius(float radius);
        IGoalFinderBuilder WithGoal<TGoal>()
            where TGoal : IGoal;

        public IPathFinder Build(in PathfindingMovementCostFactors movementProfile);
    }
}
