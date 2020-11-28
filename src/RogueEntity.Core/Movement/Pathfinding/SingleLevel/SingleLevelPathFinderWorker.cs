using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.Movement.Pathfinding.SingleLevel
{
    public class SingleLevelPathFinderWorker : AStarGridBase<IMovementMode>
    {
        static readonly ILogger Logger = SLog.ForContext<SingleLevelPathFinderWorker>();
        readonly List<Direction> directions;
        readonly List<MovementSourceData> movementCostsOnLevel;
        readonly PooledDynamicDataView2D<IMovementMode> nodesSources;
        readonly List<Position2D> pathBuffer;
        IReadOnlyBoundedDataView<DirectionalityInformation>[] directionsTile;
        IReadOnlyBoundedDataView<float>[] costsTile;

        int activeLevel;
        IPathFinderTargetEvaluator targetEvaluator;

        public SingleLevelPathFinderWorker(IBoundedDataViewPool<AStarNode> pool,
                                           IBoundedDataViewPool<IMovementMode> movementModePool) : base(pool)
        {
            pathBuffer = new List<Position2D>();
            directions = new List<Direction>();
            nodesSources = new PooledDynamicDataView2D<IMovementMode>(movementModePool);
            movementCostsOnLevel = new List<MovementSourceData>();
            directionsTile = new IReadOnlyBoundedDataView<DirectionalityInformation>[0];
            costsTile = new IReadOnlyBoundedDataView<float>[0];
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
                movementCostsOnLevel.Add(new MovementSourceData(movementCostsOnLevel.Count, costProfile, costView, directionView));
            }

            if (movementCostsOnLevel.Count > directionsTile.Length)
            {
                directionsTile = new IReadOnlyBoundedDataView<DirectionalityInformation>[movementCostsOnLevel.Count];
            }
            else
            {
                Array.Clear(directionsTile, 0, directionsTile.Length);
            }
            if (movementCostsOnLevel.Count > costsTile.Length)
            {
                costsTile = new IReadOnlyBoundedDataView<float>[movementCostsOnLevel.Count];
            }
            else
            {
                Array.Clear(costsTile, 0, costsTile.Length);
            }
            
        }

        public void ConfigureFinished(AdjacencyRule r)
        {
            directions.Clear();
            foreach (var h in r.DirectionsOfNeighbors())
            {
                directions.Add(h);
            }
        }

        public void Reset()
        {
            movementCostsOnLevel.Clear();
            nodesSources.Clear();
            Array.Clear(directionsTile, 0, directionsTile.Length);
            Array.Clear(costsTile, 0, costsTile.Length);
        }

        public PathFinderResult FindPath(EntityGridPosition from,
                                         IPathFinderTargetEvaluator evaluator,
                                         List<(EntityGridPosition, IMovementMode)> path,
                                         int maxSearchSteps = int.MaxValue)
        {
            if (from.IsInvalid)
            {
                throw new ArgumentException();
            }

            this.targetEvaluator = evaluator;

            var result = base.FindPath(from.ToGridXY(), pathBuffer, maxSearchSteps);
            if (result == PathFinderResult.NotFound)
            {
                return PathFinderResult.NotFound;
            }

            path.Clear();
            foreach (var p in pathBuffer)
            {
                path.Add((from.WithPosition(p.X, p.Y), nodesSources[p.X, p.Y]));
            }

            return result;
        }

        protected override void PopulateTraversableDirections(Position2D basePos, List<Direction> buffer)
        {
            buffer.Clear();
            
            var targetPosX = basePos.X;
            var targetPosY = basePos.Y;
            var allowedMovements = DirectionalityInformation.None;

            for (var index = 0; index < movementCostsOnLevel.Count; index++)
            {
                var s = movementCostsOnLevel[index];
                var dir = s.Directions.TryGet(ref directionsTile[s.Index], targetPosX, targetPosY, DirectionalityInformation.None);
                allowedMovements |= dir;
            }

            for (var index = 0; index < directions.Count; index++)
            {
                var d = directions[index];
                if (allowedMovements.IsMovementAllowed(d))
                {
                    buffer.Add(d);
                }
            }
        }

        protected override bool EdgeCostInformation(in Position2D sourceNode, in Direction d, float sourceNodeCost, out float totalPathCost, out IMovementMode movementMode)
        {
            var targetPosX = sourceNode.X;
            var targetPosY = sourceNode.Y;
            var costInformationAvailable = false;
            totalPathCost = 0;
            movementMode = default;

            for (var index = 0; index < movementCostsOnLevel.Count; index++)
            {
                var m = movementCostsOnLevel[index];
                var dir = m.Directions.TryGet(ref directionsTile[m.Index], targetPosX, targetPosY, DirectionalityInformation.None);
                if (dir == DirectionalityInformation.None)
                {
                    continue;
                }

                if (!dir.IsMovementAllowed(d))
                {
                    continue;
                }

                var tileCost = m.Costs.TryGet(ref costsTile[m.Index], targetPosX, targetPosY, 0);
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
            return targetEvaluator.IsTargetNode(activeLevel, in pos);
        }

        protected override float Heuristic(in Position2D pos)
        {
            return targetEvaluator.TargetHeuristic(activeLevel, in pos);
        }

        protected override void UpdateNode(in Position2D pos, IMovementMode nodeInfo)
        {
            nodesSources[pos.X, pos.Y] = nodeInfo;
        }

        readonly struct MovementSourceData
        {
            public readonly int Index;
            public readonly IMovementMode MovementType;
            public readonly float BaseCost;
            public readonly IReadOnlyDynamicDataView2D<float> Costs;
            public readonly IReadOnlyDynamicDataView2D<DirectionalityInformation> Directions;

            public MovementSourceData(int index,
                                      in MovementCost movementCost,
                                      [NotNull] IReadOnlyDynamicDataView2D<float> costs,
                                      [NotNull] IReadOnlyDynamicDataView2D<DirectionalityInformation> directions)
            {
                BaseCost = movementCost.Cost;
                MovementType = movementCost.MovementMode ?? throw new ArgumentNullException(nameof(movementCost.MovementMode));
                Index = index;
                Costs = costs ?? throw new ArgumentNullException(nameof(costs));
                Directions = directions ?? throw new ArgumentNullException(nameof(directions));
            }
        }
    }
}