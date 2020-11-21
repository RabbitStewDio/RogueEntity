using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.Pathfinding
{
    public class PathFinderSource : IPathFinderSourceBackend, IPathFinderSource
    {
        readonly struct MovementSourceData
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

        readonly Dictionary<IMovementMode, MovementSourceData> movementCostMaps;
        readonly ObjectPool<PathFinder> pathfinderPool;

        public PathFinderSource()
        {
            movementCostMaps = new Dictionary<IMovementMode, MovementSourceData>();
            pathfinderPool = new DefaultObjectPool<PathFinder>(new PathfinderPolicy(this));
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

        public IPathFinder GetPathFinder(in PathfindingMovementCostFactors movementProfile)
        {
            var pf = pathfinderPool.Get();
            foreach (var m in movementProfile.MovementCosts)
            {
                if (movementCostMaps.TryGetValue(m.MovementMode, out var mapData))
                {
                    pf.ConfigureMovementProfile(m, mapData.Costs, mapData.Directions);
                }
            }

            return pf;
        }

        public void Return(IPathFinder pf)
        {
            if (pf is PathFinder pathFinder)
            {
                pathfinderPool.Return(pathFinder);
            }
        }

        class PathfinderPolicy: IPooledObjectPolicy<PathFinder>
        {
            readonly IPathFinderSource source;
            readonly IBoundedDataViewPool<AStarNode> nodePool;
            readonly IBoundedDataViewPool<IMovementMode> movementModePool;

            public PathfinderPolicy([NotNull] IPathFinderSource source)
            {
                this.source = source ?? throw new ArgumentNullException(nameof(source));
            }

            public PathFinder Create()
            {
                return new PathFinder(source, nodePool, movementModePool);
            }

            public bool Return(PathFinder obj)
            {
                obj.Reset();
                return true;
            }
        }
    }
}