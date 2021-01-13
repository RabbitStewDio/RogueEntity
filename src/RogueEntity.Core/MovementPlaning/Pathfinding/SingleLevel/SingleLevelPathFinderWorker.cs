using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel
{
    public class SingleLevelPathFinderWorker : AStarGridBase<IMovementMode>
    {
        readonly List<MovementCostData2D> movementCostsOnLevel;
        readonly PooledDynamicDataView2D<IMovementMode> nodesSources;
        readonly BufferList<Position2D> pathBuffer;
        IReadOnlyBoundedDataView<DirectionalityInformation>[] directionsTile;
        IReadOnlyBoundedDataView<float>[] costsTile;
        IBoundedDataView<IMovementMode> nodeSourceTile;
        ReadOnlyListWrapper<Direction>[] directionData;

        int activeLevel;
        IPathFinderTargetEvaluator targetEvaluator;

        public SingleLevelPathFinderWorker(IBoundedDataViewPool<AStarNode> pool,
                                           IBoundedDataViewPool<IMovementMode> movementModePool) : base(pool)
        {
            pathBuffer = new BufferList<Position2D>();
            nodesSources = new PooledDynamicDataView2D<IMovementMode>(movementModePool);
            movementCostsOnLevel = new List<MovementCostData2D>();
            directionsTile = new IReadOnlyBoundedDataView<DirectionalityInformation>[4];
            costsTile = new IReadOnlyBoundedDataView<float>[4];
        }

        public void ConfigureActiveLevel(int z)
        {
            movementCostsOnLevel.Clear();
            nodesSources.Clear();
            nodeSourceTile = null;
            Array.Clear(directionsTile, 0, directionsTile.Length);
            Array.Clear(costsTile, 0, costsTile.Length);
            activeLevel = z;
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

        public void Reset()
        {
            nodeSourceTile = null;
            targetEvaluator = null;
            directionData = default;
            movementCostsOnLevel.Clear();
            nodesSources.Clear();
            Array.Clear(directionsTile, 0, directionsTile.Length);
            Array.Clear(costsTile, 0, costsTile.Length);
        }

        public PathFinderResult FindPath<TPosition>(TPosition from,
                                                    IPathFinderTargetEvaluator evaluator,
                                                    BufferList<(TPosition, IMovementMode)> path,
                                                    int maxSearchSteps = int.MaxValue)
            where TPosition: IPosition<TPosition>
        {
            if (from.IsInvalid)
            {
                throw new ArgumentException();
            }

            this.targetEvaluator = evaluator;

            var result = base.FindPath(from.ToGridXY(), pathBuffer, maxSearchSteps);
            path.Clear();
            foreach (var p in pathBuffer)
            {
                path.Add((from.WithPosition(p.X, p.Y), nodesSources[p.X, p.Y]));
            }

            return result;
        }

        protected override ReadOnlyListWrapper<Direction> PopulateTraversableDirections(in Position2D basePos)
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
        
        protected override bool EdgeCostInformation(in Position2D sourceNode, in Direction d, float sourceNodeCost, out float totalPathCost, out IMovementMode movementMode)
        {
            var sourcePosX = sourceNode.X;
            var sourcePosY = sourceNode.Y;
            var costInformationAvailable = false;
            totalPathCost = 0;
            movementMode = default;

            for (var index = 0; index < movementCostsOnLevel.Count; index++)
            {
                var m = movementCostsOnLevel[index];
                var dir = m.Directions.TryGet(ref directionsTile[index], sourcePosX, sourcePosY, DirectionalityInformation.None);
                if (dir == DirectionalityInformation.None)
                {
                    continue;
                }

                if (!dir.IsMovementAllowed(d))
                {
                    continue;
                }

                var targetPos = sourceNode + d;
                var sourceTileCost = m.Costs.TryGet(ref costsTile[index], sourcePosX, sourcePosY, 0);
                var targetTileCost = m.Costs.TryGet(ref costsTile[index], targetPos.X, targetPos.Y, 0);
                var tileCost = (sourceTileCost + targetTileCost) / 2.0f;
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
            IMovementMode defaultMovement = null;
            ref var entry = ref nodesSources.TryGetRefForUpdate(ref nodeSourceTile, pos.X, pos.Y, ref defaultMovement, out var success, DataViewCreateMode.CreateMissing);
            if (!success)
            {
                throw new Exception();
            }

            entry = nodeInfo;
        }

    }
}