using System;
using System.Collections.Generic;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.GoalFinding.SingleLevel
{
    public class SingleLevelGoalFinderAStarWorker : AStarGridBase<IMovementMode>, IGoalFinderTargetEvaluatorVisitor
    {
        readonly List<MovementCostData2D> movementCostsOnLevel;
        ReadOnlyListWrapper<Direction>[] directionData;
        IReadOnlyBoundedDataView<DirectionalityInformation>[] directionsTile;
        IReadOnlyBoundedDataView<float>[] costsTile;
        HashSet<Position> goals;
        int activeLevel;
        
        public SingleLevelGoalFinderAStarWorker(IBoundedDataViewPool<AStarNode> pool) : base(pool)
        {
            
        }

        protected override bool IsTargetNode(in Position2D pos)
        {
            return pos.X == 0 && pos.Y == 0; // goals.Contains(Position.Of(MapLayer.Indeterminate, pos.X, pos.Y, activeLevel));
        }

        protected override float Heuristic(in Position2D pos)
        {
            throw new System.NotImplementedException();
        }

        public void RegisterGoalAt<TGoal>(in Position pos, GoalMarker<TGoal> goal)
        {
            if (pos.GridZ == activeLevel)
            {
                goals.Add(Position.Of(MapLayer.Indeterminate, pos.X, pos.Y, activeLevel));
            }
        }

        protected override ReadOnlyListWrapper<Direction> PopulateTraversableDirections(Position2D basePos)
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
            var targetPosX = sourceNode.X;
            var targetPosY = sourceNode.Y;
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
    }
}
