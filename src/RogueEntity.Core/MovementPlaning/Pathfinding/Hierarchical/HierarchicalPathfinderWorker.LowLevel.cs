using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical;

partial class HierarchicalPathfinderWorker
{

    static DirectionalityInformation LimitMovementToZone(in ShortGridPosition2D basePos, in Rectangle zoneBounds)
    {
        var result = DirectionalityInformation.All;
        if (basePos.X == zoneBounds.MinExtentX)
        {
            result = result.WithOut(Direction.Left);
            result = result.WithOut(Direction.DownLeft);
            result = result.WithOut(Direction.UpLeft);
        }

        if (basePos.Y == zoneBounds.MinExtentY)
        {
            result = result.WithOut(Direction.Up);
            result = result.WithOut(Direction.UpLeft);
            result = result.WithOut(Direction.UpRight);
        }

        if (basePos.X == zoneBounds.MaxExtentX)
        {
            result = result.WithOut(Direction.Right);
            result = result.WithOut(Direction.UpRight);
            result = result.WithOut(Direction.DownRight);
        }

        if (basePos.X == zoneBounds.MaxExtentY)
        {
            result = result.WithOut(Direction.Down);
            result = result.WithOut(Direction.DownLeft);
            result = result.WithOut(Direction.DownRight);
        }

        return result;
    }

    protected override ReadOnlyListWrapper<Direction> PopulateTraversableDirections(ShortGridPosition2D basePos)
    {
        Assert.NotNull(zoneRegion);
        Assert.NotNull(directionData);
        Assert.NotNull(inboundDirectionsTile);
        Assert.NotNull(outboundDirectionsTile);

        var targetPosX = basePos.X + zoneRegion.Bounds.X;
        var targetPosY = basePos.Y + zoneRegion.Bounds.Y;
        var allowedMovements = DirectionalityInformation.None;

        var filter = LimitMovementToZone(basePos, zoneRegion.Bounds);

        for (var index = 0; index < movementCostsOnLevel.Count; index++)
        {
            var s = movementCostsOnLevel[index];
            ref var dt = ref outboundDirectionsTile.GetRef(index);
            var dir = s.OutboundDirections.TryGetMapValue(ref dt, targetPosX, targetPosY, DirectionalityInformation.None);
            allowedMovements |= dir;
        }

        allowedMovements &= filter;

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
        Assert.NotNull(zoneRegion);

        var origin = zoneRegion.Bounds.Position;
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
            ref var dt = ref outboundDirectionsTile.GetRef(index);

            var dir = m.OutboundDirections.TryGetMapValue(ref dt, sourcePosX, sourcePosY, DirectionalityInformation.None);
            if (dir == DirectionalityInformation.None)
            {
                continue;
            }

            if (!dir.IsMovementAllowed(d))
            {
                continue;
            }

            ref var ct = ref costsTile.GetRef(index);
            var tileCostTarget = m.Costs.TryGetMapValue(ref ct, targetPosX, targetPosY, 0);
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
        nodesSources.TrySet(pos.X, pos.Y, nodeInfo);
    }
    
}