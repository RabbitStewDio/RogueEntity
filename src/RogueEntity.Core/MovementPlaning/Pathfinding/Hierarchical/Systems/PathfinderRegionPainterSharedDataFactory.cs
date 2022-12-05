using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems
{
    public class PathfinderRegionPainterSharedDataFactory 
    {
        readonly ObjectPool<PriorityQueue<float, Position2D>> openNodesPool;

        public PathfinderRegionPainterSharedDataFactory()
        {
            openNodesPool = new DefaultObjectPool<PriorityQueue<float, Position2D>>(new PriorityQueuePolicy());
        }

        public void Return(PriorityQueue<float, Position2D> t)
        {
            openNodesPool.Return(t);
        }

        public PooledObjectHandle<PriorityQueue<float, Position2D>> Get()
        {
            return new PooledObjectHandle<PriorityQueue<float, Position2D>>(openNodesPool, openNodesPool.Get());
        }

        class PriorityQueuePolicy : IPooledObjectPolicy<PriorityQueue<float, Position2D>>
        {
            public PriorityQueue<float, Position2D> Create()
            {
                return new PriorityQueue<float, Position2D>(4096);
            }

            public bool Return(PriorityQueue<float, Position2D> obj)
            {
                obj.Clear();
                return true;
            }
        }
    }
}