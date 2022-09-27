using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel
{
    public class SingleLevelPathFinderPolicy : PooledObjectPolicy<SingleLevelPathFinder>
    {
        readonly IBoundedDataViewPool<AStarNode> nodePool;
        readonly IBoundedDataViewPool<IMovementMode> movementModePool;

        public SingleLevelPathFinderPolicy(DynamicDataViewConfiguration config) : this(new DefaultBoundedDataViewPool<AStarNode>(config),
                                                                                       new DefaultBoundedDataViewPool<IMovementMode>(config))
        {
        }

        public SingleLevelPathFinderPolicy(IBoundedDataViewPool<AStarNode>? nodePool = null,
                                           IBoundedDataViewPool<IMovementMode>? movementModePool = null)
        {
            this.nodePool = nodePool ?? new DefaultBoundedDataViewPool<AStarNode>(new DynamicDataViewConfiguration(0, 0, 16, 16));
            this.movementModePool = movementModePool ?? new DefaultBoundedDataViewPool<IMovementMode>(new DynamicDataViewConfiguration(0, 0, 16, 16));
        }

        public override SingleLevelPathFinder Create()
        {
            return new SingleLevelPathFinder(nodePool, movementModePool);
        }

        public override bool Return(SingleLevelPathFinder obj)
        {
            obj.Reset();
            return true;
        }
    }
}