using JetBrains.Annotations;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Positioning.SpatialQueries;

namespace RogueEntity.Core.Movement.GoalFinding.SingleLevel
{
    public class SingleLevelGoalFinderBuilderPolicy : IPooledObjectPolicy<SingleLevelGoalFinderBuilder>
    {
        readonly SingleLevelGoalTargetEvaluatorFactory factory;
        readonly GoalRegistry goalRegistry;
        readonly ObjectPool<SingleLevelGoalFinder> pathFinderPool;
        readonly ObjectPool<CompoundGoalTargetEvaluator> compoundTargetEvaluatorPool;

        public SingleLevelGoalFinderBuilderPolicy([NotNull] SingleLevelGoalFinderPolicy policy,
                                                  GoalRegistry goalRegistry,
                                                  ISpatialQueryLookup queryLookup)
        {
            this.goalRegistry = goalRegistry;
            this.factory = new SingleLevelGoalTargetEvaluatorFactory(goalRegistry, queryLookup);
            this.pathFinderPool = new DefaultObjectPool<SingleLevelGoalFinder>(policy);
            this.compoundTargetEvaluatorPool = new DefaultObjectPool<CompoundGoalTargetEvaluator>(new DefaultPooledObjectPolicy<CompoundGoalTargetEvaluator>());
        }

        public SingleLevelGoalFinderBuilder Create()
        {
            return new SingleLevelGoalFinderBuilder(factory, goalRegistry, pathFinderPool, compoundTargetEvaluatorPool);
        }

        public bool Return(SingleLevelGoalFinderBuilder obj)
        {
            obj.Reset();
            return true;
        }
    }
}