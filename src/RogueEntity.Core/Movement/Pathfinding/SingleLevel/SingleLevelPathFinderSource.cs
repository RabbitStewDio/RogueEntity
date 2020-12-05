using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.Pathfinding.SingleLevel
{
    public class SingleLevelPathFinderSource : IPathFinderSourceBackend, IPathFinderSource
    {
        readonly Dictionary<IMovementMode, MovementSourceData> movementCostMaps;
        readonly ObjectPool<SingleLevelPathFinderBuilder> pathfinderPool;

        public SingleLevelPathFinderSource(SingleLevelPathFinderPolicy sourcePolicy)
        {
            movementCostMaps = new Dictionary<IMovementMode, MovementSourceData>();
            pathfinderPool = new DefaultObjectPool<SingleLevelPathFinderBuilder>(new SingleLevelPathFinderBuilderPolicy(sourcePolicy));
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

        public IPathFinderBuilder GetPathFinder()
        {
            var pf = pathfinderPool.Get();
            pf.MovementCostData = movementCostMaps;

            return pf;
        }

        public void Return(IPathFinderBuilder pf)
        {
            if (pf is SingleLevelPathFinderBuilder pathFinder)
            {
                pathfinderPool.Return(pathFinder);
            }
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