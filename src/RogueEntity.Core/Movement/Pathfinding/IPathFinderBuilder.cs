using RogueEntity.Core.Movement.Cost;

namespace RogueEntity.Core.Movement.Pathfinding
{
    public interface IPathFinderBuilder
    {
        public IPathFinderBuilder WithTarget(IPathFinderTargetEvaluator evaluator);
        public IPathFinder Build(in PathfindingMovementCostFactors movementProfile);
    }
}