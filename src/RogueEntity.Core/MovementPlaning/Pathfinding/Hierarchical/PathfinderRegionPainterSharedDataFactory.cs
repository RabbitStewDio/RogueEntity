using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical
{
    public class PathfinderRegionPainterSharedDataFactory : IPooledObjectProvider<(PriorityQueue<float, Position2D>, PooledDynamicDataView2D<AStarNode>)>
    {
        readonly ObjectPool<PriorityQueue<float, Position2D>> openNodesPool;
        readonly ObjectPool<PooledDynamicDataView2D<AStarNode>> floodFillNodePool;

        public PathfinderRegionPainterSharedDataFactory(DynamicDataViewConfiguration config)
        {
            openNodesPool = new DefaultObjectPool<PriorityQueue<float, Position2D>>(new PriorityQueuePolicy());
            floodFillNodePool = new DefaultObjectPool<PooledDynamicDataView2D<AStarNode>>(new FloodFillNodePoolPolicy(config));
        }

        public void Return((PriorityQueue<float, Position2D>, PooledDynamicDataView2D<AStarNode>) t)
        {
            openNodesPool.Return(t.Item1);
            floodFillNodePool.Return(t.Item2);
        }

        public PooledObjectHandle<(PriorityQueue<float, Position2D>, PooledDynamicDataView2D<AStarNode>)> Get()
        {
            return new PooledObjectHandle<(PriorityQueue<float, Position2D>, PooledDynamicDataView2D<AStarNode>)>
                (this, (openNodesPool.Get(), floodFillNodePool.Get()));
        }

        class FloodFillNodePoolPolicy : IPooledObjectPolicy<PooledDynamicDataView2D<AStarNode>>
        {
            readonly DefaultBoundedDataViewPool<AStarNode> pool;

            public FloodFillNodePoolPolicy(DynamicDataViewConfiguration config)
            {
                this.pool = new DefaultBoundedDataViewPool<AStarNode>(config);
            }

            public PooledDynamicDataView2D<AStarNode> Create()
            {
                return new PooledDynamicDataView2D<AStarNode>(pool);
            }

            public bool Return(PooledDynamicDataView2D<AStarNode> obj)
            {
                obj.Clear();
                return true;
            }
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