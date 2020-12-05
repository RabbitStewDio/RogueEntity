namespace RogueEntity.Core.Movement.Pathfinding
{
    public interface IPathFinderSource
    {
        IPathFinderBuilder GetPathFinder();
    }
}