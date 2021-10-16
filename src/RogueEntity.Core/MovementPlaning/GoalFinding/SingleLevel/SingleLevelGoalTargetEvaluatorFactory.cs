using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.MovementPlaning.Goals;
using RogueEntity.Core.Positioning.SpatialQueries;
using System;
using System.Collections.Concurrent;

namespace RogueEntity.Core.MovementPlaning.GoalFinding.SingleLevel
{
    /// <summary>
    ///   A factory/pool class for goal evaluators. This class encapsulates and caches queries of
    ///   entities. The entity/code querying for paths towards goals should not need to know the exact types
    ///   or makeup of the goal entities. This class therefore uses a bunch of pre-made delegates to
    ///   encapsulate the generic method calls at runtime.
    /// </summary>
    public class SingleLevelGoalTargetEvaluatorFactory
    {
        readonly GoalRegistry goalRegistry;
        readonly ISpatialQueryLookup queryLookup;
        readonly ConcurrentDictionary<(Type, Type), object> data;
        readonly Func<(Type, Type), object> createDelegate;

        public SingleLevelGoalTargetEvaluatorFactory([NotNull] GoalRegistry goalRegistry, [NotNull] ISpatialQueryLookup queryLookup)
        {
            this.goalRegistry = goalRegistry ?? throw new ArgumentNullException(nameof(goalRegistry));
            this.queryLookup = queryLookup ?? throw new ArgumentNullException(nameof(queryLookup));
            data = new ConcurrentDictionary<(Type, Type), object>();
            createDelegate = Create;
        }

        public bool TryGet<TEntityId, TGoal>(out EntityGoalTargetSource2D<TEntityId, TGoal> result)
            where TEntityId : IEntityKey
        {
            var maybeResult = data.GetOrAdd((typeof(TEntityId), typeof(TGoal)), createDelegate);
            if (maybeResult is EntityGoalTargetSource2D<TEntityId, TGoal> r)
            {
                result = r;
                return true;
            }

            result = default;
            return false;
        }

        object Create((Type entityType, Type goalType) arg)
        {
            var genericLifter = new SpatialQueryLifter(queryLookup);
            return goalRegistry.LiftInstance(arg.entityType, arg.goalType, genericLifter);
        }

        class SpatialQueryLifter : IGenericLifterFunction<IEntityKey, IGoal>
        {
            readonly ISpatialQueryLookup queryLookup;

            public SpatialQueryLifter(ISpatialQueryLookup queryLookup)
            {
                this.queryLookup = queryLookup;
            }

            public object Invoke<TEntityKey, TGoal>()
                where TEntityKey : IEntityKey
                where TGoal : IGoal
            {
                if (queryLookup.TryGetQuery<TEntityKey>(out var q))
                {
                    return new EntityGoalTargetSource2D<TEntityKey, TGoal>(q);
                }

                return null;
            }
        }
    }
}