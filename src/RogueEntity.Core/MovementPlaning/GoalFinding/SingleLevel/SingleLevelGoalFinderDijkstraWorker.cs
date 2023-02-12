using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.MovementPlaning.Goals;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.MovementPlaning.GoalFinding.SingleLevel
{
    /// <summary>
    ///   Performs a localized Dijkstra-Search. All working coordinates are translated so that they center
    ///   around the search origin (origin = (0,0))
    /// </summary>
    public class SingleLevelGoalFinderDijkstraWorker : DijkstraGridBase<IMovementMode>
    {
        readonly SingleLevelPathPool pathPool;
        readonly List<MovementCostData2D> movementCostsOnLevel;
        readonly BufferList<ShortGridPosition2D> pathBuffer;

        IReadOnlyBoundedDataView<DirectionalityInformation>?[] directionsTile;
        IReadOnlyBoundedDataView<float>?[] costsTile;
        ReadOnlyListWrapper<Direction>[]? directionData;
        int activeLevel;
        readonly BoundedDataView<IMovementMode> nodesSources;
        GridPosition2D origin;

        public SingleLevelGoalFinderDijkstraWorker(SingleLevelPathPool pathPool) : base(default)
        {
            this.pathPool = pathPool ?? throw new ArgumentNullException(nameof(pathPool));
            pathBuffer = new BufferList<ShortGridPosition2D>();
            nodesSources = new BoundedDataView<IMovementMode>(default);
            movementCostsOnLevel = new List<MovementCostData2D>();
            directionsTile = Array.Empty<IReadOnlyBoundedDataView<DirectionalityInformation>>();
            costsTile = Array.Empty<IReadOnlyBoundedDataView<float>>();
        }

        public void Reset()
        {
            base.PrepareScan();
        }

        public void ConfigureActiveLevel<TPosition>(in TPosition pos, int searchRadius)
            where TPosition : IPosition<TPosition>
        {
            movementCostsOnLevel.Clear();


            var searchBounds = new Rectangle(new GridPosition2D(), searchRadius, searchRadius);
            base.Resize(searchBounds);
            nodesSources.Resize(searchBounds);
            nodesSources.Clear();

            Array.Clear(directionsTile, 0, directionsTile.Length);
            Array.Clear(costsTile, 0, costsTile.Length);
            activeLevel = pos.GridZ;
            origin = pos.ToGridXY();
        }


        public void ConfigureMovementProfile(in MovementCost costProfile,
                                             IReadOnlyDynamicDataView3D<float> movementCosts,
                                             IReadOnlyDynamicDataView3D<DirectionalityInformation> inboundMovementDirections,
                                             IReadOnlyDynamicDataView3D<DirectionalityInformation> outboundMovementDirections)
        {
            if (movementCosts.TryGetView(activeLevel, out var costView) &&
                inboundMovementDirections.TryGetView(activeLevel, out var inboundDirectionView) &&
                outboundMovementDirections.TryGetView(activeLevel, out var outboundDirectionView))
            {
                movementCostsOnLevel.Add(new MovementCostData2D(costProfile, costView, inboundDirectionView, outboundDirectionView));
            }

            if (movementCostsOnLevel.Count > directionsTile.Length)
            {
                directionsTile = new IReadOnlyBoundedDataView<DirectionalityInformation>[movementCostsOnLevel.Count];
                costsTile = new IReadOnlyBoundedDataView<float>[movementCostsOnLevel.Count];
            }
            else
            {
                Array.Clear(directionsTile, 0, directionsTile.Length);
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
                EnqueueStartingNode(new ShortGridPosition2D(p.X, p.Y), record.Strength);
            }
        }

        public bool PerformSearch<TPosition>(in TPosition from,
                                                         out (PathFinderResult resultHint, IPath path, float cost) result,
                                                         int searchLimit = int.MaxValue)
            where TPosition : IPosition<TPosition>
        {
            base.RescanMap(searchLimit);
            base.FindPath(new ShortGridPosition2D(), out _, pathBuffer);

            if (pathBuffer.Count <= 0)
            {
                result = default;
                return false;
            }

            var path = pathPool.Lease();
            var prev = pathBuffer[0];
            path.BeginRecordPath(from.ToGridXY(), from.GridZ);
            float cost = 0;
            for (var index = 1; index < pathBuffer.Count; index++)
            {
                var p = pathBuffer[index];
                var d = Directions.GetDirection(prev, p);
                EdgeCostInformation(prev, d, cost, out cost, out _);
                path.RecordStep(d, nodesSources[prev.X, prev.Y]);
                prev = p;
            }

            result = (PathFinderResult.Found, path, cost);
            return true;
        }

        protected override ReadOnlyListWrapper<Direction> PopulateTraversableDirections(ShortGridPosition2D basePos)
        {
            if (directionData == null) throw new InvalidOperationException("Configure not complete");
            
            var targetPosX = basePos.X + origin.X;
            var targetPosY = basePos.Y + origin.Y;
            var allowedMovements = DirectionalityInformation.None;

            for (var index = 0; index < movementCostsOnLevel.Count; index++)
            {
                var s = movementCostsOnLevel[index];
                var dir = s.OutboundDirections.TryGetMapValue(ref directionsTile[index], targetPosX, targetPosY, DirectionalityInformation.None);
                allowedMovements |= dir;
            }

            return directionData[(int)allowedMovements];
        }

        
        /// <summary>
        ///   GoalFinding searches from all goals to the current location of the player (inverse of the desired movement).
        ///   We have to take into account that movement options may be different in that direction, thus the edge we
        ///   compute is the edge from (source + direction to source). 
        /// </summary>
        protected override bool EdgeCostInformation(in ShortGridPosition2D sourceNode, 
                                                    in Direction d, 
                                                    float sourceNodeCost, 
                                                    out float totalPathCost, 
                                                    [MaybeNullWhen(false)] out IMovementMode movementMode)
        {
            var dx = d.ToCoordinates();
            var sourcePosX = sourceNode.X + origin.X;
            var sourcePosY = sourceNode.Y + origin.Y;
            var targetPosX = sourceNode.X + dx.X + origin.X;
            var targetPosY = sourceNode.Y + dx.Y + origin.Y;
            var costInformationAvailable = false;
            var pathCost = 0f;
            movementMode = default;

            for (var index = 0; index < movementCostsOnLevel.Count; index++)
            {
                var m = movementCostsOnLevel[index];
                var dir = m.OutboundDirections.TryGetMapValue(ref directionsTile[index], sourcePosX, sourcePosY, DirectionalityInformation.None);
                if (dir == DirectionalityInformation.None)
                {
                    continue;
                }

                if (!dir.IsMovementAllowed(d))
                {
                    continue;
                }

                var tileCostTarget = m.Costs.TryGetMapValue(ref costsTile[index], targetPosX, targetPosY, 0);
                if (tileCostTarget <= 0)
                {
                    // a cost of zero means its undefined. This should mean the tile is not valid.
                    continue;
                }

                var accumulatedCost = m.BaseCost * tileCostTarget;
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

        protected override void UpdateNode(in ShortGridPosition2D pos, IMovementMode nodeInfo)
        {
            Assert.NotNull(nodeInfo);
            nodesSources.TrySet(pos.X, pos.Y, nodeInfo);
        }
    }
}
