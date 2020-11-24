using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.Movement.Pathfinding
{
    public class SingleLevelPathFinderWorker : AStarGridBase<Position2D, IMovementMode>
    {
        static readonly ILogger Logger = SLog.ForContext<SingleLevelPathFinderWorker>();
        readonly List<Direction> directions;
        readonly List<MovementSourceData> movementCostsOnLevel;
        readonly PooledDynamicDataView2D<IMovementMode> nodesSources;
        readonly List<Position2D> pathBuffer;

        int activeLevel;
        EntityGridPosition origin;
        Position2D targetPosition;
        DistanceCalculation heuristics;

        public SingleLevelPathFinderWorker(IBoundedDataViewPool<AStarNode> pool,
                                         IBoundedDataViewPool<IMovementMode> movementModePool) : base(pool)
        {
            pathBuffer = new List<Position2D>();
            directions = new List<Direction>();
            nodesSources = new PooledDynamicDataView2D<IMovementMode>(movementModePool);
            movementCostsOnLevel = new List<MovementSourceData>();
        }

        public void ConfigureActiveLevel(int z)
        {
            activeLevel = z;
        }

        public void ConfigureMovementProfile(in MovementCost costProfile,
                                             [NotNull] IReadOnlyDynamicDataView3D<float> movementCosts,
                                             [NotNull] IReadOnlyDynamicDataView3D<DirectionalityInformation> movementDirections)
        {
            if (movementCosts.TryGetView(activeLevel, out var costView) &&
                movementDirections.TryGetView(activeLevel, out var directionView))
            {
                movementCostsOnLevel.Add(new MovementSourceData(costProfile, costView, directionView));
                if (heuristics.IsOtherMoreAccurate(costProfile.MovementStyle))
                {
                    heuristics = costProfile.MovementStyle;
                }
            }
        }

        public void ConfigureFinished()
        {
            directions.Clear();
            foreach (var h in heuristics.AsAdjacencyRule().DirectionsOfNeighbors())
            {
                directions.Add(h);
            }
        }

        public void Reset()
        {
            heuristics = DistanceCalculation.Manhattan;
            movementCostsOnLevel.Clear();
            nodesSources.Clear();
        }

        public PathFinderResult FindPath(EntityGridPosition from, 
                                         EntityGridPosition to,
                                         List<(EntityGridPosition, IMovementMode)> path,
                                         int maxSearchSteps = int.MaxValue)
        {
            if (from.IsInvalid || to.IsInvalid)
            {
                throw new ArgumentException();
            }

            if (from.GridZ != to.GridZ)
            {
                throw new ArgumentException();
            }

            origin = from;
            var dx = to.GridX - from.GridX;
            var dy = to.GridY - from.GridY;
            targetPosition = new Position2D(dx, dy);

            var result = base.FindPath(new Position2D(), pathBuffer, maxSearchSteps);
            if (result == PathFinderResult.NotFound)
            {
                return PathFinderResult.NotFound;
            }
            
            path.Clear();
            foreach (var p in pathBuffer)
            {
                path.Add((origin + p, nodesSources[p.X, p.Y]));
            }

            return result;
        }

        protected override void PopulateTraversableDirections(Position2D basePos, List<Direction> buffer)
        {
            buffer.Clear();
            var targetPos = origin + basePos;
            var allowedMovements = DirectionalityInformation.None;
            foreach (var s in movementCostsOnLevel)
            {
                if (s.Directions.TryGet(targetPos.GridX, targetPos.GridY, out var dir))
                {
                    Logger.Verbose("Direction: For {Position} is {Buffer}", targetPos, dir);
                    allowedMovements |= dir;
                }
            }

            foreach (var d in directions)
            {
                if (allowedMovements.IsMovementAllowed(d))
                {
                    buffer.Add(d);
                }
            }

            Logger.Verbose("Traversable: For {Position} is {Buffer}", targetPos, buffer);
        }

        protected override bool EdgeCostInformation(in Position2D sourceNode, in Direction d, float sourceNodeCost, out float totalPathCost, out IMovementMode movementMode)
        {
            var targetPos = origin + sourceNode;
            var costInformationAvailable = false;
            totalPathCost = 0;
            movementMode = default;
            
            foreach (var m in movementCostsOnLevel)
            {
                if (!m.Directions.TryGet(targetPos.GridX, targetPos.GridX, out var dir) || !dir.IsMovementAllowed(d))
                {
                    continue;
                }
                
                var tileCost = m.Costs[targetPos.GridX, targetPos.GridX];
                if (tileCost <= 0)
                {
                    // a cost of zero means its undefined. This should mean the tile is not valid.
                    continue;
                }
                
                var accumulatedCost = sourceNodeCost + m.BaseCost * tileCost;
                if (costInformationAvailable)
                {
                    if (accumulatedCost < totalPathCost)
                    {
                        totalPathCost = accumulatedCost;
                        movementMode = m.MovementType;
                    }
                }
                else
                {
                    totalPathCost = accumulatedCost;
                    movementMode = m.MovementType;
                    costInformationAvailable = true;
                }
            }

            return costInformationAvailable;
        }

        protected override bool IsTargetNode(in Position2D pos)
        {
            return targetPosition == pos;
        }

        protected override float Heuristic(in Position2D pos)
        {
            return (float)heuristics.Calculate2D(pos, targetPosition);
        }

        protected override void UpdateNode(in Position2D pos, IMovementMode nodeInfo)
        {
            nodesSources[pos.X, pos.Y] = nodeInfo;
        }

        readonly struct MovementSourceData
        {
            public readonly IMovementMode MovementType;
            public readonly float BaseCost;
            public readonly IReadOnlyDynamicDataView2D<float> Costs;
            public readonly IReadOnlyDynamicDataView2D<DirectionalityInformation> Directions;

            public MovementSourceData(in MovementCost movementCost,
                                      [NotNull] IReadOnlyDynamicDataView2D<float> costs,
                                      [NotNull] IReadOnlyDynamicDataView2D<DirectionalityInformation> directions)
            {
                BaseCost = movementCost.Cost;
                MovementType = movementCost.MovementMode ?? throw new ArgumentNullException(nameof(movementCost.MovementMode));
                Costs = costs ?? throw new ArgumentNullException(nameof(costs));
                Directions = directions ?? throw new ArgumentNullException(nameof(directions));
            }
        }
    }
}