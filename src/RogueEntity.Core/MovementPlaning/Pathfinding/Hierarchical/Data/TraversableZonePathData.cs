using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;

public class TraversableZonePathData
{
    readonly MovementModeEncoding modeEncoding;
    readonly ObjectPool<List<(Direction, byte)>> connectionPool;
    readonly Dictionary<GridPosition2D, InboundConnectionRecord> inboundPositions;
    readonly Dictionary<GridPosition2D, OutboundConnectionRecord> outboundPositions;
    readonly Dictionary<GridPosition2D, PathfinderZonePathSegment> connections;
    public ZoneEdgeDataKey Key { get; private set; }

    public TraversableZonePathData(MovementModeEncoding modeEncoding,
                                   ObjectPool<List<(Direction, byte)>> connectionPool)
    {
        this.modeEncoding = modeEncoding ?? throw new ArgumentNullException(nameof(modeEncoding));
        this.connectionPool = connectionPool;
        connections = new Dictionary<GridPosition2D, PathfinderZonePathSegment>();
        inboundPositions = new Dictionary<GridPosition2D, InboundConnectionRecord>();
        outboundPositions = new Dictionary<GridPosition2D, OutboundConnectionRecord>();
        Key = default;
    }

    public void Init(in ZoneEdgeDataKey key)
    {
        this.Key = key;
    }

    public bool TryGetConnection(GridPosition2D inboundPosition, GridPosition2D outboundPosition, out (MovementModeEncoding encoding, PathfinderZonePathSegment segment) connection)
    {
        if (connections.TryGetValue(inboundPosition, out var c))
        {
            connection = (modeEncoding, c);
            return true;
        }

        connection = default;
        return false;
    }
    
    public void RecordConnection(GridPosition2D inboundPosition, GridPosition2D outboundPosition, float cost, IPath path)
    {
        var compressedPath = Compress(inboundPosition, path);
        var segment = new PathfinderZonePathSegment(inboundPosition, outboundPosition, cost, compressedPath);
        connections[inboundPosition] = segment;
    }

    List<(Direction, byte)> Compress(GridPosition2D inboundPosition,
                                                IPath path)
    {
        var data = new List<(Direction, byte)>();
        var prevPos = inboundPosition;
        foreach (var (pathPos, pathMode) in path)
        {
            if (!modeEncoding.TryGetModeIndex(pathMode, out var idx))
            {
                throw new ArgumentException();
            }

            var dir = Directions.GetDirection(prevPos, pathPos.ToGridXY());
            data.Add((dir, idx));
        }

        return data;
    }

    public void RemoveInboundConnection(PathfinderRegionEdge edgeSpec)
    {
        var pos = edgeSpec.EdgeSource + edgeSpec.EdgeTargetDirection;
        if (inboundPositions.TryGetValue(pos, out var edges))
        {
            edges.inboundEdges.Remove(edgeSpec);
        }
    }

    public void RemoveOutboundConnection(PathfinderRegionEdge edgeSpec)
    {
        var pos = edgeSpec.EdgeSource;
        if (outboundPositions.TryGetValue(pos, out var edges))
        {
            edges.outboundConnections.Remove(edgeSpec);
        }
    }

    public void Clear()
    {
        inboundPositions.Clear();
        outboundPositions.Clear();
        foreach (var c in connections)
        {
            connectionPool.Return(c.Value.TraversalSteps);
        }

        connections.Clear();
    }

    public BufferList<(GridPosition2D pos, OutboundConnectionRecord record)> GetOutboundConnections(BufferList<(GridPosition2D pos, OutboundConnectionRecord record)>? buffer = null)
    {
        buffer = BufferList.PrepareBuffer(buffer);
        foreach (var b in outboundPositions)
        {
            buffer.Add((b.Key, b.Value));
        }

        return buffer;
    }

    public BufferList<(GridPosition2D pos, InboundConnectionRecord record)> GetInboundConnections(BufferList<(GridPosition2D pos, InboundConnectionRecord record)>? buffer = null)
    {
        buffer = BufferList.PrepareBuffer(buffer);
        foreach (var b in inboundPositions)
        {
            buffer.Add((b.Key, b.Value));
        }

        return buffer;
    }

    public void AddOutboundEdge(in PathfinderRegionEdge edge)
    {
        if (!outboundPositions.TryGetValue(edge.EdgeSource, out var data))
        {
            data = new OutboundConnectionRecord();
            outboundPositions[edge.EdgeSource] = data;
        }

        data.AddOutboundEdge(edge);
    }

    public void AddInboundEdge(in PathfinderRegionEdge edge)
    {
        var pos = edge.EdgeSource + edge.EdgeTargetDirection;
        if (!inboundPositions.TryGetValue(pos, out var data))
        {
            data = new InboundConnectionRecord();
            inboundPositions[pos] = data;
        }

        data.AddInboundEdge(edge);
    }
}