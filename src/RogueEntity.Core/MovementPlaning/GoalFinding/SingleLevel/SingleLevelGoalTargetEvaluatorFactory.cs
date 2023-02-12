using EnTTSharp.Entities;
using RogueEntity.Core.MovementPlaning.Goals;
using RogueEntity.Core.Positioning.SpatialQueries;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
        readonly ISpatialQueryLookup queryLookup;
        readonly Dictionary<(Type, Type), object> data;

        public SingleLevelGoalTargetEvaluatorFactory(ISpatialQueryLookup queryLookup)
        {
            this.data = new Dictionary<(Type, Type), object>();
            this.queryLookup = queryLookup ?? throw new ArgumentNullException(nameof(queryLookup));
        }

        public bool TryGet<TEntityId, TGoal>([MaybeNullWhen(false)] out EntityGoalTargetSource2D<TEntityId, TGoal> result)
            where TEntityId : struct, IEntityKey 
            where TGoal : IGoal
        {
            lock (data)
            {
                if (data.TryGetValue((typeof(TEntityId), typeof(TGoal)), out var maybeResult) && (maybeResult is EntityGoalTargetSource2D<TEntityId, TGoal> r))
                {
                    result = r;
                    return true;
                }

                if (queryLookup.TryGetQuery<TEntityId, GoalMarker<TGoal>>(out var q))
                {
                    result = new EntityGoalTargetSource2D<TEntityId, TGoal>(q);
                    data[(typeof(TEntityId), typeof(TGoal))] = result;
                    return true;
                }
            }

            result = default;
            return false;
        }
    }
}