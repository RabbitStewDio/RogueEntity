using RogueEntity.Core.Utils;

namespace RogueEntity.Core.MovementPlaning.Pathfinding
{
    /// <summary>
    ///   A injectable factory to create pathfinder builders, which in return configure
    ///   path finder instances. 
    /// </summary>
    public interface IPathFinderSource
    {
        PooledObjectHandle<IPathFinderBuilder> GetPathFinder();
    }
}