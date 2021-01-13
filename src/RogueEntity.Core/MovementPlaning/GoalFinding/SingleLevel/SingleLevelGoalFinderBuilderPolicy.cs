using JetBrains.Annotations;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.MovementPlaning.Goals;
using RogueEntity.Core.MovementPlaning.Goals.Filters;
using RogueEntity.Core.Positioning.SpatialQueries;

namespace RogueEntity.Core.MovementPlaning.GoalFinding.SingleLevel
{
    public class SingleLevelGoalFinderBuilderPolicy : IPooledObjectPolicy<SingleLevelGoalFinderBuilder>
    {
        readonly SingleLevelGoalTargetEvaluatorFactory factory;
        readonly GoalRegistry goalRegistry;
        readonly ObjectPool<SingleLevelGoalFinder> pathFinderPool;
        readonly ObjectPool<CompoundGoalTargetSource> compoundTargetEvaluatorPool;
        readonly ObjectPool<AnyOfGoalFinderFilter> filterPool;

        public SingleLevelGoalFinderBuilderPolicy([NotNull] SingleLevelGoalFinderPolicy policy,
                                                  GoalRegistry goalRegistry,
                                                  ISpatialQueryLookup queryLookup)
        {
            this.goalRegistry = goalRegistry;
            this.factory = new SingleLevelGoalTargetEvaluatorFactory(goalRegistry, queryLookup);
            this.pathFinderPool = new DefaultObjectPool<SingleLevelGoalFinder>(policy);
            this.compoundTargetEvaluatorPool = new DefaultObjectPool<CompoundGoalTargetSource>(new DefaultPooledObjectPolicy<CompoundGoalTargetSource>());
            this.filterPool = new DefaultObjectPool<AnyOfGoalFinderFilter>(new DefaultPooledObjectPolicy<AnyOfGoalFinderFilter>());
        }

        public SingleLevelGoalFinderBuilder Create()
        {
            return new SingleLevelGoalFinderBuilder(factory, goalRegistry, pathFinderPool, compoundTargetEvaluatorPool, filterPool);
        }

        public bool Return(SingleLevelGoalFinderBuilder obj)
        {
            obj.Reset();
            return true;
        }
    }
}