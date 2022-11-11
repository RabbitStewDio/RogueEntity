using RogueEntity.Core.Movement;
using RogueEntity.Core.Positioning.Algorithms;
using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;

public readonly struct OutboundConnectionId : IEquatable<OutboundConnectionId>
{
    readonly Direction direction;
    readonly DistanceCalculation distanceCalculation;
    readonly IMovementMode movementMode;

    public OutboundConnectionId(Direction direction, DistanceCalculation distanceCalculation, IMovementMode movementMode)
    {
        this.direction = direction;
        this.distanceCalculation = distanceCalculation;
        this.movementMode = movementMode;
    }

    public bool Equals(OutboundConnectionId other)
    {
        return direction == other.direction && distanceCalculation == other.distanceCalculation && movementMode.Equals(other.movementMode);
    }

    public override bool Equals(object? obj)
    {
        return obj is OutboundConnectionId other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (int)direction;
            hashCode = (hashCode * 397) ^ (int)distanceCalculation;
            hashCode = (hashCode * 397) ^ movementMode.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(OutboundConnectionId left, OutboundConnectionId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(OutboundConnectionId left, OutboundConnectionId right)
    {
        return !left.Equals(right);
    }
}