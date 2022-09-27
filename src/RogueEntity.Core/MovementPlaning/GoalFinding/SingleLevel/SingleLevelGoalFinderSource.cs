using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Movement;
using RogueEntity.Core.MovementPlaning.Goals;
using RogueEntity.Core.Positioning.SpatialQueries;

namespace RogueEntity.Core.MovementPlaning.GoalFinding.SingleLevel
{
    public class SingleLevelGoalFinderSource : IGoalFinderSource
    {
        readonly IMovementDataProvider dataProvider;
        readonly DefaultObjectPool<SingleLevelGoalFinderBuilder> pool;

        public SingleLevelGoalFinderSource(IPooledObjectPolicy<SingleLevelGoalFinder> policy,
                                           GoalRegistry registry,
                                           ISpatialQueryLookup queryLookUp,
                                           IMovementDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
            pool = new DefaultObjectPool<SingleLevelGoalFinderBuilder>(new SingleLevelGoalFinderBuilderPolicy(policy,
                                                                                                              registry,
                                                                                                              queryLookUp));
        }

        public IGoalFinderBuilder GetGoalFinder()
        {
            var x = pool.Get();
            x.Configure(dataProvider.MovementCosts);
            return x;
        }
    }
}