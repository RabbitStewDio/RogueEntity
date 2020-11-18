namespace RogueEntity.Core.Movement.Pathfinding
{
    public interface IPathFinderPool
    {
        public IPathFinder Reserve();
        public void ReturnToPool(IPathFinder p);
    }
}