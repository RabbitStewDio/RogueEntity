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
    public class SingleLevelPathFinderSource : IPathFinderSourceBackend, IPathFinderSource
    {
        readonly Dictionary<IMovementMode, MovementSourceData> movementCostMaps;
        readonly ObjectPool<SingleLevelPathFinder> pathfinderPool;

        public SingleLevelPathFinderSource(SingleLevelPathfinderPolicy policy = null)
        {
            movementCostMaps = new Dictionary<IMovementMode, MovementSourceData>();
            pathfinderPool = new DefaultObjectPool<SingleLevelPathFinder>(policy ?? new SingleLevelPathfinderPolicy(this));
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
            if (pf is SingleLevelPathFinder pathFinder)
            {
                pathfinderPool.Return(pathFinder);
            }
        }

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
    }
    
    public class SingleLevelPathfinderPolicy : IPooledObjectPolicy<SingleLevelPathFinder>
    {
        readonly IPathFinderSource source;
        readonly IBoundedDataViewPool<AStarNode> nodePool;
        readonly IBoundedDataViewPool<IMovementMode> movementModePool;

        public SingleLevelPathfinderPolicy([NotNull] IPathFinderSource source, DynamicDataViewConfiguration config) : this(source, 
                                                                                                                           new DefaultBoundedDataViewPool<AStarNode>(config),
                                                                                                                           new DefaultBoundedDataViewPool<IMovementMode>(config))
        {
        }

        public SingleLevelPathfinderPolicy(IPathFinderSource source, IBoundedDataViewPool<AStarNode> nodePool = null, IBoundedDataViewPool<IMovementMode> movementModePool = null)
        {
            this.source = source;
            this.nodePool = nodePool ?? new DefaultBoundedDataViewPool<AStarNode>(new DynamicDataViewConfiguration(0, 0, 16, 16));
            this.movementModePool = movementModePool ?? new DefaultBoundedDataViewPool<IMovementMode>(new DynamicDataViewConfiguration(0, 0, 16, 16));
        }

        public SingleLevelPathFinder Create()
        {
            return new SingleLevelPathFinder(source, nodePool, movementModePool);
        }

        public bool Return(SingleLevelPathFinder obj)
        {
            obj.Reset();
            return true;
        }
    }

}