using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems
{
    public class PathfinderRegionPainterSharedDataFactory : IPooledObjectProvider<PriorityQueue<float, Position2D>>
    {
        readonly ObjectPool<PriorityQueue<float, Position2D>> openNodesPool;

        public PathfinderRegionPainterSharedDataFactory(DynamicDataViewConfiguration config)
        {
            openNodesPool = new DefaultObjectPool<PriorityQueue<float, Position2D>>(new PriorityQueuePolicy());
        }

        public void Return(PriorityQueue<float, Position2D> t)
        {
            openNodesPool.Return(t);
        }

        public PooledObjectHandle<PriorityQueue<float, Position2D>> Get()
        {
            return new PooledObjectHandle<PriorityQueue<float, Position2D>>
                (this, openNodesPool.Get());
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