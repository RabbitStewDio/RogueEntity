using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel
{
    public class SingleLevelPathFinderSource : IPathFinderSource, IPooledObjectProvider<IPathFinderBuilder>
    {
        readonly ObjectPool<SingleLevelPathFinderBuilder> pathfinderPool;        

        public SingleLevelPathFinderSource(SingleLevelPathFinderPolicy sourcePolicy, IMovementDataProvider dataProvider)
        {
            pathfinderPool = new DefaultObjectPool<SingleLevelPathFinderBuilder>(new SingleLevelPathFinderBuilderPolicy(dataProvider, sourcePolicy));
        }

        public PooledObjectHandle<IPathFinderBuilder> GetPathFinder()
        {
            var pf = pathfinderPool.Get();
            return new PooledObjectHandle<IPathFinderBuilder>(this, pf);
        }

        public void Return(IPathFinderBuilder pf)
        {
            if (pf is SingleLevelPathFinderBuilder pathFinder)
            {
                pathfinderPool.Return(pathFinder);
            }
        }

    }
}