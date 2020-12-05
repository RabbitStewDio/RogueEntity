using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.Pathfinding.SingleLevel
{
    public class SingleLevelPathFinderPolicy : IPooledObjectPolicy<SingleLevelPathFinder>
    {
        readonly IBoundedDataViewPool<AStarNode> nodePool;
        readonly IBoundedDataViewPool<IMovementMode> movementModePool;

        public SingleLevelPathFinderPolicy(DynamicDataViewConfiguration config) : this(new DefaultBoundedDataViewPool<AStarNode>(config),
                                                                                       new DefaultBoundedDataViewPool<IMovementMode>(config))
        {
        }

        public SingleLevelPathFinderPolicy(IBoundedDataViewPool<AStarNode> nodePool = null,
                                           IBoundedDataViewPool<IMovementMode> movementModePool = null)
        {
            this.nodePool = nodePool ?? new DefaultBoundedDataViewPool<AStarNode>(new DynamicDataViewConfiguration(0, 0, 16, 16));
            this.movementModePool = movementModePool ?? new DefaultBoundedDataViewPool<IMovementMode>(new DynamicDataViewConfiguration(0, 0, 16, 16));
        }

        public SingleLevelPathFinder Create()
        {
            return new SingleLevelPathFinder(nodePool, movementModePool);
        }

        public bool Return(SingleLevelPathFinder obj)
        {
            obj.Reset();
            return true;
        }
    }
}