using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;

/// <summary>
///    Records a transition between traversable zones across regions. A region edge is always a record of
///    a location of a possible transition between traversable zones in one direction (from the origin/owner
///    zone to its neighbour). The neighbour is referenced as global target region.    
/// </summary>
public readonly struct PathfinderRegionEdge : IEquatable<PathfinderRegionEdge>
{
    public readonly GlobalTraversableZoneId OwnerId;
    public readonly GlobalTraversableZoneId TargetZone;
    public readonly EdgeId LocalEdgeId;
    
    /// <summary>
    ///    The entry point into the target zone.
    /// </summary>
    public readonly Direction EdgeTargetDirection;
    public readonly GridPosition2D EdgeSource;
    public readonly GridPosition2D EdgeTarget => EdgeSource + EdgeTargetDirection;

    public PathfinderRegionEdge(GlobalTraversableZoneId ownerId, 
                                EdgeId localEdgeId, 
                                GridPosition2D edgeSource, 
                                Direction edgeTarget, 
                                GlobalTraversableZoneId targetZone)
    {
        if (ownerId.ZoneId == TraversableZoneId.Empty) throw new ArgumentException();
        OwnerId = ownerId;
        LocalEdgeId = localEdgeId;
        EdgeSource = edgeSource;
        EdgeTargetDirection = edgeTarget;
        TargetZone = targetZone;
    }

    public bool Equals(PathfinderRegionEdge other)
    {
        return OwnerId.Equals(other.OwnerId) && LocalEdgeId.Equals(other.LocalEdgeId) && 
               EdgeTargetDirection == other.EdgeTargetDirection && EdgeSource.Equals(other.EdgeSource);
    }

    public override bool Equals(object? obj)
    {
        return obj is PathfinderRegionEdge other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = OwnerId.GetHashCode();
            hashCode = (hashCode * 397) ^ LocalEdgeId.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)EdgeTargetDirection;
            hashCode = (hashCode * 397) ^ EdgeSource.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(PathfinderRegionEdge left, PathfinderRegionEdge right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PathfinderRegionEdge left, PathfinderRegionEdge right)
    {
        return !left.Equals(right);
    }

    public PathfinderRegionEdge WithSourcePosition(GridPosition2D pos, Direction d)
    {
        return new PathfinderRegionEdge(OwnerId, LocalEdgeId, pos, d, TargetZone);
    }

    public override string ToString()
    {
        return $"PathfinderRegionEdge({nameof(OwnerId)}: {OwnerId}, {nameof(LocalEdgeId)}: {LocalEdgeId}, {nameof(EdgeTargetDirection)}: {EdgeTargetDirection}, {nameof(TargetZone)}: {TargetZone})";
    }
}