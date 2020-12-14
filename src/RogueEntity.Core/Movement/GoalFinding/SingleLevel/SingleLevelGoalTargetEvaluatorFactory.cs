using System;
using System.Collections.Concurrent;
using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Movement.Goals;
using RogueEntity.Core.Positioning.SpatialQueries;

namespace RogueEntity.Core.Movement.GoalFinding.SingleLevel
{
    /// <summary>
    ///   A factory/pool class for goal evaluators. 
    /// </summary>
    public class SingleLevelGoalTargetEvaluatorFactory
    {
        readonly GoalRegistry goalRegistry;
        readonly ISpatialQueryLookup queryLookup;
        readonly ConcurrentDictionary<(Type, Type), object> data;
        readonly Func<(Type, Type), object> createDelegate;

        public SingleLevelGoalTargetEvaluatorFactory(GoalRegistry goalRegistry, ISpatialQueryLookup queryLookup)
        {
            this.goalRegistry = goalRegistry;
            this.queryLookup = queryLookup;
            data = new ConcurrentDictionary<(Type, Type), object>();
            createDelegate = Create;
        }

        public bool TryGet<TEntityId, TGoal>(out GoalTargetEvaluator2D<TEntityId, TGoal> result)
            where TEntityId : IEntityKey
        {
            var maybeResult = data.GetOrAdd((typeof(TEntityId), typeof(TGoal)), createDelegate);
            if (maybeResult is GoalTargetEvaluator2D<TEntityId, TGoal> r)
            {
                result = r;
                return true;
            }

            result = default;
            return false;
        }

        object Create((Type, Type) arg)
        {
            var genericLifter = new Lifter(queryLookup);
            return goalRegistry.LiftInstance(arg.Item1, arg.Item2, genericLifter);
        }

        class Lifter : IGenericLifterFunction<IEntityKey, IGoal>
        {
            readonly ISpatialQueryLookup queryLookup;

            public Lifter(ISpatialQueryLookup queryLookup)
            {
                this.queryLookup = queryLookup;
            }

            public object Invoke<TContextA, TContextB>()
                where TContextA : IEntityKey
                where TContextB : IGoal
            {
                if (queryLookup.TryGetQuery<TContextA>(out var q))
                {
                    return new GoalTargetEvaluator2D<TContextA, TContextB>(q);
                }

                return null;
            }
        }
    }
}