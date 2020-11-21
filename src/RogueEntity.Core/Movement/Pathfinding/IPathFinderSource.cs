using RogueEntity.Core.Movement.Cost;

namespace RogueEntity.Core.Movement.Pathfinding
{
    public interface IPathFinderSource
    {
        IPathFinder GetPathFinder(in PathfindingMovementCostFactors movementProfile);
        void Return(IPathFinder pf);
    }
}