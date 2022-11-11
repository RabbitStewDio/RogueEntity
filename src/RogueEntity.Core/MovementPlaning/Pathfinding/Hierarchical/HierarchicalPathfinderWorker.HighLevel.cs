using EnTTSharp;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical;

partial class HierarchicalPathfinderWorker
{
    public bool FindPath(Optional<(IPath path, float maxCost)> directPath, 
                         IPathFinder fragmentPathfinder,
                         [MaybeNullWhen(false)] out HierarchicalPath path,
                         out float pathCost)
    {
        Assert.NotNull(targetEvaluator);
        
        if (!PopulateStartingNodes(directPath))
        {
            path = default;
            pathCost = default;
            return false;
        }

        if (!StartHighLevelPathFinding(directPath, out var targetNode))
        {
            path = default;
            pathCost = default;
            return false;
        }

        // we have at least a path into the target node.
        if (!RecordPathSegment(out var startSegment, out var startSegmentCost))
        {
            path = default;
            pathCost = default;
            return false;
        }

        var result = new HierarchicalPath();
        result.BeginRecordPath(originPos, z);
        result.AddSegment(startSegment);

        var totalCost = startSegmentCost;

        using var segmentBuffer = BufferListPool<(IPath path, float cost)>.GetPooled();
        var segments = CollectSegments(targetNode, segmentBuffer);
        for (var i = segments.Count - 1; i >= 0; i--)
        {
            var (segment, cost) = segments[i];
            result.AddSegment(segment);
            totalCost += cost;
        }
        
        // find path for end segment ...
        var edgeEdgeTarget = targetNode.Edge.EdgeTarget;
        if (!targetEvaluator.IsTargetNode(z, edgeEdgeTarget))
        {
            if (fragmentPathfinder.TryFindPath(EntityGridPosition.Of(MapLayer.Indeterminate, edgeEdgeTarget.X, edgeEdgeTarget.Y, z), 
                                               out var fragment))
            {
                result.AddSegment(fragment.path);
                totalCost += fragment.pathCost;
            }
            else
            {
                result.Dispose();
                path = default;
                pathCost = default;
                return false;
            }
        }
        
        pathCost = totalCost;
        path = result;
        return true;
    }

    bool PopulateStartingNodes(Optional<(IPath path, float maxCost)> directPath)
    {
        PrepareScan();

        if (!data.ZoneDataView.TryGetRegionView2D(z, out var zoneView2D) ||
            !zoneView2D.TryGetRegion(originPos.X, originPos.Y, out zoneRegion))
        {
            return false;
        }

        if (!data.EdgeDataView.TryGetView(z, out var edgeView2D) ||
            !edgeView2D.TryGetView(originPos, out edgeRegion))
        {
            return false;
        }

        nodesSources.Resize(zoneRegion.Bounds);
        nodesSources.Clear();

        EnqueueStartingNode(zoneRegion.ToInternalPosition(originPos), 1);
        RescanMap();

        float maxCost = float.MaxValue;
        if (directPath.TryGetValue(out var path))
        {
            maxCost = path.maxCost;
        }

        using var zoneBuffer = BufferListPool<TraversableZonePathData>.GetPooled();
        using var recordBuffer = BufferListPool<(Position2D, OutboundConnectionRecord)>.GetPooled();
        using var edgeBuffer = BufferListPool<PathfinderRegionEdge>.GetPooled();
        bool haveStartingNode = false;
        foreach (var zone in edgeRegion.GetZoneData(originZone, zoneBuffer))
        {
            var k = zone.Key;
            foreach (var b in zone.GetOutboundConnections(recordBuffer))
            {
                foreach (var edge in b.record.GetEdges(edgeBuffer))
                {
                    if (TryGetCumulativeCost(zoneRegion.ToInternalPosition(edge.EdgeSource), out var cost) &&
                        b.record.outboundConnections.TryGetValue(edge, out var step))
                    {
                        var totalCost = cost + step;
                        if (totalCost >= maxCost)
                        {
                            continue;
                        }

                        var h = Heuristic(edge.EdgeTarget);
                        openNodesHighLevel.Enqueue(new HighLevelNode(edge, k, totalCost, h), totalCost);
                        haveStartingNode = true;
                    }
                }
            }
        }

        return haveStartingNode;
    }


    bool StartHighLevelPathFinding(Optional<(IPath path, float maxCost)> directPath, 
                                   out HighLevelNode result)
    {
        using var nodeBuffer = BufferListPool<HighLevelNode>.GetPooled();
        float maxCost = float.MaxValue;
        if (directPath.TryGetValue(out var p))
        {
            maxCost = p.maxCost;
        }

        while (openNodesHighLevel.Count > 0)
        {
            var openNode = openNodesHighLevel.Dequeue();
            var openNodePos = openNode.Edge.EdgeTarget;
            if (closedNodesHighLevel.TryGetValue(openNodePos, out var closedNode))
            {
                // this is a previously closed node
                // lets check if this node is worse than what we already have 
                if (closedNode.IsBetterThan(openNode, movementCosts))
                {
                    continue;
                }
            }

            closedNodesHighLevel[openNodePos] = openNode;
            if (targetsByZones.ContainsKey(openNode.Edge.TargetZone))
            {
                // found a valid target
                result = openNode;
                return true;
            }

            foreach (var nodeCandidate in PopulateOutgoingEdges(openNode.Edge, openNode.Cost, nodeBuffer))
            {
                if (closedNodesHighLevel.TryGetValue(nodeCandidate.Edge.EdgeTarget, out var closed) && 
                    closed.IsBetterThan(nodeCandidate, movementCosts))
                {
                    continue;
                }
                
                if (nodeCandidate.Cost > maxCost)
                {
                    continue;
                }

                openNodesHighLevel.Enqueue(nodeCandidate, nodeCandidate.Cost);
            }
        }

        result = default;
        return false;
    }

    BufferList<HighLevelNode> PopulateOutgoingEdges(PathfinderRegionEdge inboundEdge,
                                                    float costSoFar,
                                                    BufferList<HighLevelNode>? result)
    {
        Assert.NotNull(edgeData2D);

        result = BufferList.PrepareBuffer(result);

        using var zoneBuffer = BufferListPool<TraversableZonePathData>.GetPooled();
        using var outboundRecordBuffer = BufferListPool<(Position2D, OutboundConnectionRecord)>.GetPooled();
        using var edgeBuffer = BufferListPool<PathfinderRegionEdge>.GetPooled();
        foreach (var zone in edgeData2D.GetZoneData(inboundEdge.TargetZone, zoneBuffer))
        {
            var key = zone.Key;
            foreach (var outRecord in zone.GetOutboundConnections(outboundRecordBuffer))
            {
                if (!zone.TryGetConnection(inboundEdge.EdgeTarget, outRecord.pos, out var connectionRaw))
                {
                    continue;
                }

                var connection = connectionRaw.segment;
                foreach (var edge in outRecord.record.GetEdges(edgeBuffer))
                {
                    if (edge.EdgeTarget == inboundEdge.EdgeSource)
                    {
                        // filter out the inbound edge.
                        continue;
                    }

                    if (outRecord.record.outboundConnections.TryGetValue(edge, out var lastStep))
                    {
                        var h = Heuristic(edge.EdgeTarget);
                        result.Add(new HighLevelNode(inboundEdge.EdgeTarget, edge, key, costSoFar + connection.Cost + lastStep, h));
                    }
                }
            }
        }

        return result;
    }

    float Heuristic(Position2D pos)
    {
        if (targetEvaluator == null) return 0;
        return targetEvaluator.TargetHeuristic(z, pos);
    }


    bool RecordPathSegment([MaybeNullWhen(false)] out IPath result, out float segmentCost)
    {
        Assert.NotNull(zoneRegion);
        
        using var pathBufferHandle = BufferListPool<ShortPosition2D>.GetPooled();
        var pathBuffer = base.FindPath(zoneRegion.ToInternalPosition(originPos), out _, pathBufferHandle);

        if (pathBuffer.Count <= 0)
        {
            result = default;
            segmentCost = default;
            return false;
        }

        var path = pathPool.Lease();
        var prev = pathBuffer[0];
        path.BeginRecordPath(originPos, z);
        float cost = 0;
        for (var index = 1; index < pathBuffer.Count; index++)
        {
            var p = pathBuffer[index];
            var d = Directions.GetDirection(prev, p);
            EdgeCostInformation(prev, d, cost, out cost, out _);
            path.RecordStep(d, nodesSources[prev.X, prev.Y]);
            prev = p;
        }

        segmentCost = cost;
        result = path;
        return true;

    }

    BufferList<(IPath path, float cost)> CollectSegments(HighLevelNode target, 
                                                         BufferList<(IPath path, float cost)>? buffer = null)
    {
        
        buffer = BufferList.PrepareBuffer(buffer);

        while (true)
        {
            if (!TryFindBestPath(target, out var path, out var nextNode))
            {
                break;
            }

            target = nextNode;
            buffer.Add((path, target.Cost));
        }
        
        return buffer;
    }

    bool TryFindBestPath(HighLevelNode target, 
                         [MaybeNullWhen(false)] out IPath path, out HighLevelNode parent)
    {
        Assert.NotNull(edgeData2D);
        
        var targetPos = target.Edge.EdgeTarget;
        if (!target.ParentPosition.TryGetValue(out var parentPos))
        {
            // must be a starting node. 
            path = default;
            parent = default;
            return false;
        }

        if (!closedNodesHighLevel.TryGetValue(parentPos, out var parentNode))
        {
            // WARNING: I expected to find a reference here
            path = default;
            parent = default;
            return false;
        }

        // find cheapest path from parentPos to targetPos
        Optional<(MovementModeEncoding encoder, PathfinderZonePathSegment segment)> cheapestSegment = default;
        foreach (var r in edgeData2D.GetZoneData(target.Edge.OwnerId))
        {
            var k = r.Key;
            if (!r.TryGetConnection(parentPos, targetPos, out var segment))
            {
                continue;
            }

            if (cheapestSegment.TryGetValue(out var existingSegment))
            {
                if (existingSegment.segment.Cost > segment.segment.Cost)
                {
                    cheapestSegment = segment;
                }
            }
            else
            {
                cheapestSegment = segment;
            }
        }

        if (!cheapestSegment.TryGetValue(out var cheap))
        {
            // WARNING: have not found a connection?
            path = default;
            parent = default;
            return false;
        }

        var pathRaw = pathPool.Lease();
        cheap.segment.PopulatePath(pathRaw, z, cheap.encoder);
        pathRaw.RecordStep(target.Edge.EdgeTargetDirection, null);
        path = pathRaw;
        parent = parentNode;
        return true;
    }
    
    readonly struct HighLevelNode : IEquatable<HighLevelNode>
    {
        public readonly Optional<Position2D> ParentPosition;
        public readonly PathfinderRegionEdge Edge;
        public readonly ZoneEdgeDataKey Key;
        public readonly float Cost;
        public readonly float Heuristic;

        public HighLevelNode(Position2D parent, PathfinderRegionEdge edge, ZoneEdgeDataKey key, float cost, float heuristic)
        {
            this.Edge = edge;
            this.Key = key;
            this.Cost = cost;
            this.Heuristic = heuristic;
            this.ParentPosition = parent;
        }

        public HighLevelNode(PathfinderRegionEdge edge, ZoneEdgeDataKey key, float cost, float heuristic)
        {
            this.Edge = edge;
            this.Key = key;
            this.Cost = cost;
            this.Heuristic = heuristic;
            this.ParentPosition = default;
        }

        public bool Equals(HighLevelNode other)
        {
            return Edge.Equals(other.Edge) && Key.Equals(other.Key);
        }

        public override bool Equals(object? obj)
        {
            return obj is HighLevelNode other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Edge.GetHashCode() * 397) ^ Key.GetHashCode();
            }
        }

        public static bool operator ==(HighLevelNode left, HighLevelNode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HighLevelNode left, HighLevelNode right)
        {
            return !left.Equals(right);
        }

        public bool IsBetterThan(HighLevelNode node, Dictionary<IMovementMode, MovementCost> movementPreference)
        {
            if (Cost > node.Cost)
            {
                return false;
            }

            if (Cost < node.Cost)
            {
                return true;
            }

            if (movementPreference.TryGetValue(Key.MovementMode, out var openMovement) &&
                movementPreference.TryGetValue(node.Key.MovementMode, out var closedMovement))
            {
                if (openMovement.Preference < closedMovement.Preference)
                {
                    return true;
                }
            }

            return false;
        }
    }
}