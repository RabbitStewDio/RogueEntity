using RogueEntity.Core.Movement.Cost;

namespace RogueEntity.Core.MovementPlaning.Pathfinding
{
    public interface IPathFinderBuilder
    {
        public IPathFinderBuilder WithTarget(IPathFinderTargetEvaluator evaluator);
        public IPathFinder Build(in AggregateMovementCostFactors movementProfile);
    }
}