using RogueEntity.Core.Movement;
using RogueEntity.Core.Positioning.Algorithms;
using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;

public readonly struct ZoneEdgeDataKey : IEquatable<ZoneEdgeDataKey>
{
    public readonly GlobalTraversableZoneId ZoneId;
    public readonly DistanceCalculation MovementStyle;
    public readonly IMovementMode MovementMode;

    public ZoneEdgeDataKey(GlobalTraversableZoneId zoneId, IMovementMode movementMode, DistanceCalculation movementStyle)
    {
        this.MovementMode = movementMode;
        this.MovementStyle = movementStyle;
        this.ZoneId = zoneId;
    }

    public void Deconstruct(out GlobalTraversableZoneId zoneId, out DistanceCalculation movementStyle, out IMovementMode movementMode)
    {
        zoneId = this.ZoneId;
        movementStyle = this.MovementStyle;
        movementMode = this.MovementMode;
    }

    public bool Equals(ZoneEdgeDataKey other)
    {
        return MovementMode.Equals(other.MovementMode) && MovementStyle == other.MovementStyle && ZoneId.Equals(other.ZoneId);
    }

    public override bool Equals(object? obj)
    {
        return obj is ZoneEdgeDataKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = MovementMode.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)MovementStyle;
            hashCode = (hashCode * 397) ^ ZoneId.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(ZoneEdgeDataKey left, ZoneEdgeDataKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ZoneEdgeDataKey left, ZoneEdgeDataKey right)
    {
        return !left.Equals(right);
    }
}