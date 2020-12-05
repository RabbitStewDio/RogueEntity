using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.GoalFinding.SingleLevel
{
    public class SingleLevelGoalFinderWorker : DijkstraGridBase<IMovementMode>, IGoalFinderTargetEvaluatorVisitor
    {
        readonly List<MovementCostData2D> movementCostsOnLevel;
        readonly List<ShortPosition2D> pathBuffer;

        IReadOnlyBoundedDataView<DirectionalityInformation>[] directionsTile;
        IReadOnlyBoundedDataView<float>[] costsTile;
        ReadOnlyListWrapper<Direction>[] directionData;
        int activeLevel;
        readonly BoundedDataView<IMovementMode> nodesSources;
        Position2D origin;

        public void Reset()
        {
            base.PrepareScan();
        }

        public SingleLevelGoalFinderWorker() : base(default)
        {
            pathBuffer = new List<ShortPosition2D>();
            nodesSources = new BoundedDataView<IMovementMode>(default);
            movementCostsOnLevel = new List<MovementCostData2D>();
        }

        public void ConfigureActiveLevel<TPosition>(in TPosition pos, in Rectangle searchBounds)
            where TPosition : IPosition<TPosition>
        {
            movementCostsOnLevel.Clear();

            nodesSources.Resize(searchBounds);
            nodesSources.Clear();

            Array.Clear(directionsTile, 0, directionsTile.Length);
            Array.Clear(costsTile, 0, costsTile.Length);
            activeLevel = pos.GridZ;
            origin = pos.ToGridXY();
        }


        public void ConfigureMovementProfile(in MovementCost costProfile,
                                             [NotNull] IReadOnlyDynamicDataView3D<float> movementCosts,
                                             [NotNull] IReadOnlyDynamicDataView3D<DirectionalityInformation> movementDirections)
        {
            if (movementCosts.TryGetView(activeLevel, out var costView) &&
                movementDirections.TryGetView(activeLevel, out var directionView))
            {
                movementCostsOnLevel.Add(new MovementCostData2D(costProfile, costView, directionView));
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
            directionData = DirectionalityLookup.Get(r);
        }

        public void RegisterGoalAt<TGoal>(in Position pos, GoalMarker<TGoal> goal)
        {
            if (pos.GridZ != activeLevel) return;

            var p = pos.ToGridXY() - origin;
            if (nodesSources.Contains(p.X, p.Y))
            {
                EnqueueStartingNode(new ShortPosition2D(p.X, p.Y), goal.Strength);
            }
        }

        public PathFinderResult PerformSearch<TPosition>(in TPosition from,
                                                         List<(TPosition, IMovementMode)> path)
            where TPosition : IPosition<TPosition>
        {
            base.RescanMap();
            base.FindPath(new ShortPosition2D(), out _, pathBuffer);
            path.Clear();
            foreach (var p in pathBuffer)
            {
                path.Add((from.WithPosition(p.X, p.Y), nodesSources[p.X, p.Y]));
            }

            if (path.Count == 0)
            {
                return PathFinderResult.NotFound;
            }
            
            return PathFinderResult.Found;
        }

        protected override ReadOnlyListWrapper<Direction> PopulateTraversableDirections(ShortPosition2D basePos)
        {
            var targetPosX = basePos.X;
            var targetPosY = basePos.Y;
            var allowedMovements = DirectionalityInformation.None;

            for (var index = 0; index < movementCostsOnLevel.Count; index++)
            {
                var s = movementCostsOnLevel[index];
                var dir = s.Directions.TryGet(ref directionsTile[index], targetPosX, targetPosY, DirectionalityInformation.None);
                allowedMovements |= dir;
            }

            return directionData[(int)allowedMovements];
        }

        protected override bool EdgeCostInformation(in ShortPosition2D sourceNode, in Direction d, float sourceNodeCost, out float totalPathCost, out IMovementMode movementMode)
        {
            var targetPosX = sourceNode.X;
            var targetPosY = sourceNode.Y;
            var costInformationAvailable = false;
            totalPathCost = 0;
            movementMode = default;

            for (var index = 0; index < movementCostsOnLevel.Count; index++)
            {
                var m = movementCostsOnLevel[index];
                var dir = m.Directions.TryGet(ref directionsTile[index], targetPosX, targetPosY, DirectionalityInformation.None);
                if (dir == DirectionalityInformation.None)
                {
                    continue;
                }

                if (!dir.IsMovementAllowed(d))
                {
                    continue;
                }

                var tileCost = m.Costs.TryGet(ref costsTile[index], targetPosX, targetPosY, 0);
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

        protected override void UpdateNode(in ShortPosition2D pos, IMovementMode nodeInfo)
        {
            nodesSources.TrySet(pos.X, pos.Y, nodeInfo);
        }
    }
}
