using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;

/// <summary>
///    Connects all newly calculated zones.
/// </summary>
public readonly struct PathfinderRegionEdgeDetectorPostProcessor
{
    readonly PathfinderRegionDataView zoneData;
    readonly PathfinderRegionEdgeData2D edgeData2D;
    readonly PathfinderRegionEdgeData edgeData;

    public PathfinderRegionEdgeDetectorPostProcessor(PathfinderRegionDataView zoneData,
                                                     PathfinderRegionEdgeData2D edgeData2D,
                                                     PathfinderRegionEdgeData edgeData)
    {
        this.zoneData = zoneData;
        this.edgeData2D = edgeData2D;
        this.edgeData = edgeData;
    }

    public void ReconnectZoneEdges()
    {
        using var connectionBuffer = BufferListPool<(Position2D, OutboundConnectionRecord)>.GetPooled();
        using var pathDataBuffer = BufferListPool<TraversableZonePathData>.GetPooled();
        using var zoneIdBuffer = BufferListPool<GlobalTraversableZoneId>.GetPooled();
        var regionId = zoneData.Bounds.Position;
        foreach (var z in this.edgeData.GetZones(zoneIdBuffer))
        {
            if (!edgeData.IsDirty(z))
            {
                continue;
            }
            
            foreach (var pathData in this.edgeData.GetZoneData(z, pathDataBuffer))
            {
                var (_, style, mode) = pathData.Key;
                foreach (var (_, record) in pathData.GetOutboundConnections(connectionBuffer))
                {
                    foreach (var edge in record.outboundConnections.Keys)
                    {
                        if (edge.TargetZone.RegionId == regionId)
                        {
                            edgeData.AddInboundEdge(style, mode, edge);
                        }
                        else
                        {
                            edgeData2D.AddInboundEdge(style, mode, edge);
                        }
                    }
                }
            }
        }
    }

}