using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;
using RogueEntity.Core.Utils;
using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;

/// <summary>
///    Removes old edges and their inbound counter parts.
/// </summary>
public readonly struct PathfinderRegionEdgeDetectorPreparationJob
{
    readonly PathfinderRegionDataView region;
    readonly PathfinderRegionEdgeData2D edgeData2D;
    readonly PathfinderRegionEdgeData edgeData;

    public PathfinderRegionEdgeDetectorPreparationJob(PathfinderRegionDataView region,
                                                      PathfinderRegionEdgeData2D edgeData2D,
                                                      PathfinderRegionEdgeData edgeData)
    {
        this.region = region;
        this.edgeData2D = edgeData2D;
        this.edgeData = edgeData;
    }

    public void Process()
    {
        if (region.State == RegionEdgeState.Clean)
        {
            return;
        }

        if (region.State == RegionEdgeState.Dirty)
        {
            MarkZonesAsDirty(region.Bounds);
            return;
        }

        if ((region.State & RegionEdgeState.EdgeEast) == RegionEdgeState.EdgeEast)
        {
            ReconnectEdge(RegionEdgeState.EdgeEast);
        }

        if ((region.State & RegionEdgeState.EdgeWest) == RegionEdgeState.EdgeWest)
        {
            ReconnectEdge(RegionEdgeState.EdgeWest);
        }

        if ((region.State & RegionEdgeState.EdgeNorth) == RegionEdgeState.EdgeNorth)
        {
            ReconnectEdge(RegionEdgeState.EdgeNorth);
        }

        if ((region.State & RegionEdgeState.EdgeSouth) == RegionEdgeState.EdgeSouth)
        {
            ReconnectEdge(RegionEdgeState.EdgeSouth);
        }
    }

    void ReconnectEdge(RegionEdgeState direction)
    {
        var bounds = region.Bounds;
        switch (direction)
        {
            case RegionEdgeState.EdgeNorth:
            {
                MarkZonesAsDirty(new Rectangle(bounds.X, bounds.Y, bounds.Width, 1));
                break;
            }
            case RegionEdgeState.EdgeEast:
            {
                MarkZonesAsDirty(new Rectangle(bounds.MaxExtentX, bounds.Y, 1, bounds.Height));
                break;
            }
            case RegionEdgeState.EdgeSouth:
            {
                MarkZonesAsDirty(new Rectangle(bounds.X, bounds.MaxExtentY, bounds.Width, 1));
                break;
            }
            case RegionEdgeState.EdgeWest:
            {
                MarkZonesAsDirty(new Rectangle(bounds.X, bounds.Y, 1, bounds.Height));
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }

    public void MarkZonesAsDirty(Rectangle bounds)
    {
        using var buffer = BufferListPool<TraversableZonePathData>.GetPooled(); 
        using var inboundBuffer = BufferListPool<(Position2D, InboundConnectionRecord)>.GetPooled(); 
        using var outboundBuffer = BufferListPool<(Position2D, OutboundConnectionRecord)>.GetPooled(); 
        // we are about to recompute the zone's outbound connections.
        // first remove all old outbound connections and their inbound counterparts at the target zone
        foreach (var pos in bounds.Contents)
        {
            
            if (!region.TryGet(pos, out var zone))
            {
                continue;
            }

            var globalId = new GlobalTraversableZoneId(region.Bounds.Position, zone.zone);
            foreach (var data in edgeData.GetZoneData(globalId, buffer))
            {
                foreach (var (_, b) in data.GetInboundConnections(inboundBuffer))
                {
                    foreach (var x in b.inboundEdges)
                    {
                        RemoveOutboundEdge(x);
                    }
                }

                foreach (var (_, outboundData) in data.GetOutboundConnections(outboundBuffer))
                {
                    foreach (var edge in outboundData.outboundConnections.Keys)
                    {
                        RemoveInboundEdge(edge);
                    }
                }
                
                data.Clear();
            }

        }
    }

    void RemoveOutboundEdge(PathfinderRegionEdge edge)
    {
        using var buffer = BufferListPool<TraversableZonePathData>.GetPooled();
        foreach (var z in edgeData2D.GetZoneData(edge.OwnerId))
        {
            z.RemoveOutboundConnection(edge);
        }
    }

    void RemoveInboundEdge(PathfinderRegionEdge edge)
    {
        using var buffer = BufferListPool<TraversableZonePathData>.GetPooled();
        foreach (var z in edgeData2D.GetZoneData(edge.TargetZone))
        {
            z.RemoveInboundConnection(edge);
        }
    }
}