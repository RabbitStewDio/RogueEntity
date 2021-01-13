namespace RogueEntity.Core.MovementPlaning.Pathfinding
{
    /// <summary>
    ///   A injectable factory to create pathfinder builders, which in return configure
    ///   path finder instances. 
    /// </summary>
    public interface IPathFinderSource
    {
        IPathFinderBuilder GetPathFinder();
    }
}