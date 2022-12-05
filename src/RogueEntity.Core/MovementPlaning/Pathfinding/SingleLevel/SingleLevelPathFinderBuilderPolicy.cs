using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Movement;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel
{
    class SingleLevelPathFinderBuilderPolicy : IPooledObjectPolicy<SingleLevelPathFinderBuilder>
    {
        readonly IMovementDataProvider movementDataProvider;
        readonly ObjectPool<SingleLevelPathFinder> pathFinderPool;

        public SingleLevelPathFinderBuilderPolicy(IMovementDataProvider movementDataProvider, 
                                                  IPooledObjectPolicy<SingleLevelPathFinder> policy)
        {
            this.movementDataProvider = movementDataProvider;
            this.pathFinderPool = new DefaultObjectPool<SingleLevelPathFinder>(policy);
        }

        public SingleLevelPathFinderBuilder Create()
        {
            return new SingleLevelPathFinderBuilder(movementDataProvider, pathFinderPool);
        }

        public bool Return(SingleLevelPathFinderBuilder obj)
        {
            obj.Reset();
            return true;
        }
    }
}