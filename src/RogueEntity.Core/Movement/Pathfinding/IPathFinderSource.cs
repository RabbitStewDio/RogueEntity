using RogueEntity.Core.Movement.Cost;

namespace RogueEntity.Core.Movement.Pathfinding
{
    public interface IPathFinderSource
    {
        IPathFinderBuilder GetPathFinder();
        void Return(IPathFinderBuilder pf);
    }
}