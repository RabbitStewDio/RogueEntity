using RogueEntity.Core.Utils;
using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;

public readonly struct GlobalTraversableZoneId : IEquatable<GlobalTraversableZoneId>
{
    public readonly GridPosition2D RegionId;
    public readonly TraversableZoneId ZoneId;

    public GlobalTraversableZoneId(GridPosition2D regionId, TraversableZoneId zoneId)
    {
        RegionId = regionId;
        ZoneId = zoneId;
    }

    public bool Equals(GlobalTraversableZoneId other)
    {
        return RegionId.Equals(other.RegionId) && ZoneId.Equals(other.ZoneId);
    }

    public override bool Equals(object? obj)
    {
        return obj is GlobalTraversableZoneId other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (RegionId.GetHashCode() * 397) ^ ZoneId.GetHashCode();
        }
    }

    public static bool operator ==(GlobalTraversableZoneId left, GlobalTraversableZoneId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GlobalTraversableZoneId left, GlobalTraversableZoneId right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"GlobalTraversableZoneId({RegionId}, {ZoneId})";
    }
}