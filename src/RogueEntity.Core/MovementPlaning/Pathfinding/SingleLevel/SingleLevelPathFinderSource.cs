using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Movement;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel
{
    public class SingleLevelPathFinderSource : IPathFinderSource
    {
        readonly IMovementDataProvider dataProvider;
        readonly ObjectPool<SingleLevelPathFinderBuilder> pathfinderPool;

        public SingleLevelPathFinderSource(SingleLevelPathFinderPolicy sourcePolicy, IMovementDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
            pathfinderPool = new DefaultObjectPool<SingleLevelPathFinderBuilder>(new SingleLevelPathFinderBuilderPolicy(sourcePolicy));
        }

        public IPathFinderBuilder GetPathFinder()
        {
            var pf = pathfinderPool.Get();
            pf.MovementCostData = dataProvider.MovementCosts;

            return pf;
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