using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Movement.Goals;
using RogueEntity.Core.Positioning.SpatialQueries;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.GoalFinding.SingleLevel
{
    public class SingleLevelGoalFinderSource : IGoalFinderSource
    {
        readonly Dictionary<IMovementMode, MovementSourceData> movementCostMaps;
        readonly DefaultObjectPool<SingleLevelGoalFinderBuilder> pool;

        public SingleLevelGoalFinderSource(SingleLevelGoalFinderPolicy policy,
                                           GoalRegistry registry,
                                           ISpatialQueryLookup queryLookUp)
        {
            movementCostMaps = new Dictionary<IMovementMode, MovementSourceData>();
            pool = new DefaultObjectPool<SingleLevelGoalFinderBuilder>(new SingleLevelGoalFinderBuilderPolicy(policy,
                                                                                                              registry,
                                                                                                              queryLookUp));
        }

        public void RegisterMovementSource(IMovementMode movementMode,
                                           IReadOnlyDynamicDataView3D<float> cost,
                                           IReadOnlyDynamicDataView3D<DirectionalityInformation> direction)
        {
            if (cost == null)
            {
                throw new ArgumentNullException(nameof(cost));
            }

            if (direction == null)
            {
                throw new ArgumentNullException(nameof(direction));
            }

            movementCostMaps[movementMode] = new MovementSourceData(cost, direction);
        }

        public IGoalFinderBuilder GetGoalFinder()
        {
            var x = pool.Get();
            x.Configure(movementCostMaps);
            return x;
        }

        public readonly struct MovementSourceData
        {
            public readonly IReadOnlyDynamicDataView3D<float> Costs;
            public readonly IReadOnlyDynamicDataView3D<DirectionalityInformation> Directions;

            public MovementSourceData([NotNull] IReadOnlyDynamicDataView3D<float> costs,
                                      [NotNull] IReadOnlyDynamicDataView3D<DirectionalityInformation> directions)
            {
                Costs = costs ?? throw new ArgumentNullException(nameof(costs));
                Directions = directions ?? throw new ArgumentNullException(nameof(directions));
            }
        }
    }
}