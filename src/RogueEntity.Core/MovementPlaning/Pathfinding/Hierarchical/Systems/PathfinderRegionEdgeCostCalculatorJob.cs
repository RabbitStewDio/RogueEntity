using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;

/// <summary>
///   A worker task that connects all edges of a given region with its reachable zones.
/// </summary>
public readonly struct PathfinderRegionEdgeCostCalculatorJob
{
    readonly IPathFinderBuilder pathfinderBuilder;
    readonly int z;
    readonly DistanceCalculation movementType;
    readonly PathfinderRegionEdgeData region;
    readonly MovementSourceData movementCostData;

    public PathfinderRegionEdgeCostCalculatorJob(PathfinderRegionEdgeData region,
                                                 IPathFinderBuilder pathfinderBuilder,
                                                 int z,
                                                 MovementSourceData movementCostData,
                                                 DistanceCalculation movementType)
    {
        this.region = region;
        this.pathfinderBuilder = pathfinderBuilder;
        this.z = z;
        this.movementCostData = movementCostData;
        this.movementType = movementType;
    }

    public void Process()
    {
        using var zoneIdBuffer = BufferListPool<GlobalTraversableZoneId>.GetPooled();
        using var inboundConnectionBuffer = BufferListPool<(Position2D, InboundConnectionRecord)>.GetPooled();
        using var outboundConnectionBuffer = BufferListPool<(Position2D, OutboundConnectionRecord)>.GetPooled();
        
        using var pathBuffer = BufferListPool<Position2D>.GetPooled();
        using var targetEvaluator = DefaultPathFinderTargetEvaluator.GetSharedInstance();
        using var movementCosts = BufferListPool<MovementCost>.GetPooled();
        using var pathDataBuffer = BufferListPool<(EntityGridPosition, IMovementMode)>.GetPooled();
        using var edgeBuffer = BufferListPool<PathfinderRegionEdge>.GetPooled();
        
        IReadOnlyBoundedDataView<float>? costTile = null;
        if (!movementCostData.Costs.TryGetView(z, out var costView))
        {
            return;
        }

        movementCosts.Data.Clear();
        movementCosts.Data.Add(new MovementCost(movementCostData.MovementMode, movementType, 0));

        using var pathFinder = pathfinderBuilder.Build(new AggregateMovementCostFactors(movementCosts.Data));

        foreach (var zone in region.GetZones(zoneIdBuffer))
        {
            if (!region.IsDirty(zone))
            {
                continue;
            }

            if (!region.TryGetZone(zone, movementType, movementCostData.MovementMode, out var pathData))
            {
                continue;
            }

            foreach (var (targetPos, rec) in pathData.GetOutboundConnections(outboundConnectionBuffer))
            {
                foreach (var (sourcePos, _) in pathData.GetInboundConnections(inboundConnectionBuffer))
                {
                    pathFinder.WithTarget(targetEvaluator.WithTargetPosition(EntityGridPosition.Of(MapLayer.Indeterminate, targetPos.X, targetPos.Y, z)));
                    if (pathFinder.TryFindPath(EntityGridPosition.Of(MapLayer.Indeterminate, sourcePos.X, sourcePos.Y, z),
                                                                  out var path))
                    {
                        var (resultHint, resultPath, resultCost) = path;
                        pathData.RecordConnection(sourcePos, targetPos, resultCost, resultPath);
                    }
                }

                
                foreach (var connection in rec.GetEdges(edgeBuffer))
                {
                    var targetCell = connection.EdgeSource + connection.EdgeTargetDirection;
                    var x = costView.TryGetMapValue(ref costTile, targetCell.X, targetCell.Y, 0);
                    rec.outboundConnections[connection] = x;
                }
            }
        }

        region.MarkClean();
    }
}