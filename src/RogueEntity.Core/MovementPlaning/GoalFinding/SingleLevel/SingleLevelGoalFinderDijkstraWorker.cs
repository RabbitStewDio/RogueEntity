using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.MovementPlaning.Goals;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.GoalFinding.SingleLevel
{
    /// <summary>
    ///   Performs a localized Dijkstra-Search. All working coordinates are translated so that they center
    ///   around the search origin (origin = (0,0))
    /// </summary>
    public class SingleLevelGoalFinderDijkstraWorker : DijkstraGridBase<IMovementMode>
    {
        readonly List<MovementCostData2D> movementCostsOnLevel;
        readonly BufferList<ShortPosition2D> pathBuffer;

        IReadOnlyBoundedDataView<DirectionalityInformation>[] directionsTile;
        IReadOnlyBoundedDataView<float>[] costsTile;
        ReadOnlyListWrapper<Direction>[] directionData;
        int activeLevel;
        readonly BoundedDataView<IMovementMode> nodesSources;
        Position2D origin;

        public SingleLevelGoalFinderDijkstraWorker() : base(default)
        {
            pathBuffer = new BufferList<ShortPosition2D>();
            nodesSources = new BoundedDataView<IMovementMode>(default);
            movementCostsOnLevel = new List<MovementCostData2D>();
            directionsTile = new IReadOnlyBoundedDataView<DirectionalityInformation>[0];
            costsTile = new IReadOnlyBoundedDataView<float>[0];
        }

        public void Reset()
        {
            base.PrepareScan();
        }

        public void ConfigureActiveLevel<TPosition>(in TPosition pos, int searchRadius)
            where TPosition : IPosition<TPosition>
        {
            movementCostsOnLevel.Clear();


            var searchBounds = new Rectangle(new Position2D(), searchRadius, searchRadius);
            base.Resize(searchBounds);
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

        public void AddGoal(in GoalRecord record)
        {
            var pos = record.Position;
            if (pos.GridZ != activeLevel) return;

            var p = pos.ToGridXY() - origin;
            if (nodesSources.Contains(p.X, p.Y))
            {
                EnqueueStartingNode(new ShortPosition2D(p.X, p.Y), record.Strength);
            }
        }

        public PathFinderResult PerformSearch<TPosition>(in TPosition from,
                                                         BufferList<(TPosition, IMovementMode)> path,
                                                         int searchLimit = int.MaxValue)
            where TPosition : IPosition<TPosition>
        {
            base.RescanMap(searchLimit);
            
            base.FindPath(new ShortPosition2D(), out _, pathBuffer);
            path.Clear();
            foreach (var p in pathBuffer)
            {
                path.Add((from.WithPosition(p.X + origin.X, p.Y + origin.Y), nodesSources[p.X, p.Y]));
            }
            path.Reverse();

            if (path.Count == 0)
            {
                return PathFinderResult.NotFound;
            }
            
            return PathFinderResult.Found;
        }

        protected override ReadOnlyListWrapper<Direction> PopulateTraversableDirections(ShortPosition2D basePos)
        {
            var targetPosX = basePos.X + origin.X;
            var targetPosY = basePos.Y + origin.Y;
            var allowedMovements = DirectionalityInformation.None;

            for (var index = 0; index < movementCostsOnLevel.Count; index++)
            {
                var s = movementCostsOnLevel[index];
                var dir = s.Directions.TryGet(ref directionsTile[index], targetPosX, targetPosY, DirectionalityInformation.None);
                allowedMovements |= dir;
            }

            return directionData[(int)allowedMovements];
        }

        
        /// <summary>
        ///   GoalFinding searches from all goals to the current location of the player (inverse of the desired movement).
        ///   We have to take into account that movement options may be different in that direction, thus the edge we
        ///   compute is the edge from (source + direction to source). 
        /// </summary>
        protected override bool EdgeCostInformation(in ShortPosition2D sourceNode, in Direction d, float sourceNodeCost, out float totalPathCost, out IMovementMode movementMode)
        {
            var inverseDirection = d.Inverse();
            var dx = d.ToCoordinates();
            var targetPosX = sourceNode.X + dx.X + origin.X;
            var targetPosY = sourceNode.Y + dx.Y + origin.Y;
            var costInformationAvailable = false;
            var pathCost = 0f;
            movementMode = default;

            for (var index = 0; index < movementCostsOnLevel.Count; index++)
            {
                var m = movementCostsOnLevel[index];
                var dir = m.Directions.TryGet(ref directionsTile[index], targetPosX, targetPosY, DirectionalityInformation.None);
                if (dir == DirectionalityInformation.None)
                {
                    continue;
                }

                if (!dir.IsMovementAllowed(inverseDirection))
                {
                    continue;
                }

                var tileCost = m.Costs.TryGet(ref costsTile[index], targetPosX, targetPosY, 0);
                if (tileCost <= 0)
                {
                    // a cost of zero means its undefined. This should mean the tile is not valid.
                    continue;
                }

                var accumulatedCost = m.BaseCost * tileCost;
                if (costInformationAvailable)
                {
                    if (accumulatedCost < pathCost)
                    {
                        pathCost = accumulatedCost;
                        movementMode = m.MovementType;
                    }
                }
                else
                {
                    pathCost = accumulatedCost;
                    movementMode = m.MovementType;
                    costInformationAvailable = true;
                }
            }

            totalPathCost = Math.Max(0, sourceNodeCost - pathCost);
            return costInformationAvailable;
        }

        protected override void UpdateNode(in ShortPosition2D pos, IMovementMode nodeInfo)
        {
            nodesSources.TrySet(pos.X, pos.Y, nodeInfo);
        }
    }
}
