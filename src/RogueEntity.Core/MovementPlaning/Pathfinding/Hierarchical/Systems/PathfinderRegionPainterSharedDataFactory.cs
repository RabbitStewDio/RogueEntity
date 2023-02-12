using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems
{
    public class PathfinderRegionPainterSharedDataFactory 
    {
        readonly ObjectPool<PriorityQueue<float, GridPosition2D>> openNodesPool;

        public PathfinderRegionPainterSharedDataFactory()
        {
            openNodesPool = new DefaultObjectPool<PriorityQueue<float, GridPosition2D>>(new PriorityQueuePolicy());
        }

        public void Return(PriorityQueue<float, GridPosition2D> t)
        {
            openNodesPool.Return(t);
        }

        public PooledObjectHandle<PriorityQueue<float, GridPosition2D>> Get()
        {
            return new PooledObjectHandle<PriorityQueue<float, GridPosition2D>>(openNodesPool, openNodesPool.Get());
        }

        class PriorityQueuePolicy : IPooledObjectPolicy<PriorityQueue<float, GridPosition2D>>
        {
            public PriorityQueue<float, GridPosition2D> Create()
            {
                return new PriorityQueue<float, GridPosition2D>(4096);
            }

            public bool Return(PriorityQueue<float, GridPosition2D> obj)
            {
                obj.Clear();
                return true;
            }
        }
    }
}