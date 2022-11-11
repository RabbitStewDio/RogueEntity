using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel
{
    public class SingleLevelPathFinderWorker : AStarGridBase<IMovementMode>
    {
        readonly List<MovementCostData2D> movementCostsOnLevel;
        readonly PooledDynamicDataView2D<IMovementMode> nodesSources;
        readonly BufferList<Position2D> pathBuffer;
        readonly BufferList<IReadOnlyBoundedDataView<DirectionalityInformation>?> inboundDirectionsTile;
        readonly BufferList<IReadOnlyBoundedDataView<DirectionalityInformation>?> outboundDirectionsTile;
        readonly BufferList<IReadOnlyBoundedDataView<float>?> costsTile;
        IBoundedDataView<IMovementMode>? nodeSourceTile;
        ReadOnlyListWrapper<Direction>[] directionData;

        int activeLevel;
        IPathFinderTargetEvaluator? targetEvaluator;

        public SingleLevelPathFinderWorker(IBoundedDataViewPool<AStarNode> pool,
                                           IBoundedDataViewPool<IMovementMode> movementModePool) : base(pool)
        {
            pathBuffer = new BufferList<Position2D>();
            nodesSources = new PooledDynamicDataView2D<IMovementMode>(movementModePool);
            movementCostsOnLevel = new List<MovementCostData2D>();
            inboundDirectionsTile = new BufferList<IReadOnlyBoundedDataView<DirectionalityInformation>?>();
            outboundDirectionsTile = new BufferList<IReadOnlyBoundedDataView<DirectionalityInformation>?>();
            costsTile = new BufferList<IReadOnlyBoundedDataView<float>?>();
            directionData = DirectionalityLookup.Get(AdjacencyRule.EightWay);
        }

        public void ConfigureActiveLevel(int z)
        {
            movementCostsOnLevel.Clear();
            nodesSources.Clear();
            nodeSourceTile = null;
            inboundDirectionsTile.Clear();
            outboundDirectionsTile.Clear();
            costsTile.Clear();
            activeLevel = z;
        }

        public void ConfigureMovementProfile(in MovementCostData3D m)
        {
            if (m.TryGetMovementData2D(activeLevel, out var move2D))
            {
                movementCostsOnLevel.Add(move2D);
            }
        }

        public void ConfigureFinished(AdjacencyRule r)
        {
            outboundDirectionsTile.Clear();
            inboundDirectionsTile.Clear();
            costsTile.Clear();
            outboundDirectionsTile.EnsureSizeNullable(movementCostsOnLevel.Count);
            inboundDirectionsTile.EnsureSizeNullable(movementCostsOnLevel.Count);
            costsTile.EnsureSizeNullable(movementCostsOnLevel.Count);

            directionData = DirectionalityLookup.Get(r);
        }

        public void Reset()
        {
            nodeSourceTile = null;
            targetEvaluator = null;
            movementCostsOnLevel.Clear();
            nodesSources.Clear();
            outboundDirectionsTile.Clear();
            inboundDirectionsTile.Clear();
            costsTile.Clear();
        }

        public (PathFinderResult, float cost) FindPath<TPosition>(TPosition from,
                                                                  IPathFinderTargetEvaluator evaluator,
                                                                  SingleLevelPath path,
                                                                  int maxSearchSteps = int.MaxValue)
            where TPosition : IPosition<TPosition>
        {
            if (from.IsInvalid)
            {
                throw new ArgumentException();
            }

            this.targetEvaluator = evaluator;

            var (result, totalCost) = base.FindPath(from.ToGridXY(), pathBuffer, maxSearchSteps);
            path.BeginRecordPath(from.ToGridXY(), from.GridZ);
            var position = from.ToGridXY();
            for (var i = pathBuffer.Count - 1; i >= 0; i--)
            {
                var p = pathBuffer[i];
                var d = Directions.GetDirection(position, p);
                position = p;
                path.RecordStep(d, nodesSources[p.X, p.Y]);
            }

            return (result, totalCost);
        }

        protected override ReadOnlyListWrapper<Direction> PopulateTraversableDirections(in Position2D basePos)
        {
            Assert.NotNull(directionData);

            var targetPosX = basePos.X;
            var targetPosY = basePos.Y;
            var allowedMovements = DirectionalityInformation.None;

            for (var index = 0; index < movementCostsOnLevel.Count; index++)
            {
                var s = movementCostsOnLevel[index];
                ref var dt = ref outboundDirectionsTile.GetRef(index);
                var dir = s.OutboundDirections.TryGetMapValue(ref dt, targetPosX, targetPosY, DirectionalityInformation.None);
                allowedMovements |= dir;
            }

            return directionData[(int)allowedMovements];
        }

        protected override bool EdgeCostInformation(in Position2D sourceNode,
                                                    in Direction d,
                                                    float sourceNodeCost,
                                                    out float totalPathCost,
                                                    [MaybeNullWhen(false)] out IMovementMode movementMode)
        {
            var sourcePosX = sourceNode.X;
            var sourcePosY = sourceNode.Y;
            var costInformationAvailable = false;
            totalPathCost = 0;
            movementMode = default;

            for (var index = 0; index < movementCostsOnLevel.Count; index++)
            {
                var m = movementCostsOnLevel[index];
                var dir = m.OutboundDirections.TryGetMapValue(ref outboundDirectionsTile.GetRef(index), sourcePosX, sourcePosY, DirectionalityInformation.None);
                if (dir == DirectionalityInformation.None)
                {
                    continue;
                }

                if (!dir.IsMovementAllowed(d))
                {
                    continue;
                }

                var targetPos = sourceNode + d;
                var targetTileCost = m.Costs.TryGetMapValue(ref costsTile.GetRef(index), targetPos.X, targetPos.Y, 0);
                if (targetTileCost <= 0)
                {
                    // a cost of zero means its undefined. This should mean the tile is not valid.
                    continue;
                }

                var accumulatedCost = sourceNodeCost + m.BaseCost * targetTileCost;
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
            return targetEvaluator?.IsTargetNode(activeLevel, in pos) ?? false;
        }

        protected override float Heuristic(in Position2D pos)
        {
            return targetEvaluator?.TargetHeuristic(activeLevel, in pos) ?? 0;
        }

        protected override void UpdateNode(in Position2D pos, IMovementMode nodeInfo)
        {
            IMovementMode? defaultMovement = null;
            ref var entry = ref nodesSources.TryGetRefForUpdate(ref nodeSourceTile, pos.X, pos.Y, ref defaultMovement, out var success, DataViewCreateMode.CreateMissing);
            if (!success)
            {
                throw new Exception();
            }

            entry = nodeInfo;
        }
    }
}