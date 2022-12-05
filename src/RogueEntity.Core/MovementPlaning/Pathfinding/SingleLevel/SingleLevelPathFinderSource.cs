using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel
{
    public class SingleLevelPathFinderSource : IPathFinderSource
    {
        readonly ObjectPool<IPathFinderBuilder> pathfinderPool;        

        public SingleLevelPathFinderSource(SingleLevelPathFinderPolicy sourcePolicy, IMovementDataProvider dataProvider)
        {
            var policy = new SingleLevelPathFinderBuilderPolicy(dataProvider, sourcePolicy);
            pathfinderPool = new DefaultObjectPool<IPathFinderBuilder>(policy.DownGrade<IPathFinderBuilder, SingleLevelPathFinderBuilder>());
        }

        public PooledObjectHandle<IPathFinderBuilder> GetPathFinder()
        {
            var pf = pathfinderPool.Get();
            return new PooledObjectHandle<IPathFinderBuilder>(pathfinderPool, pf);
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