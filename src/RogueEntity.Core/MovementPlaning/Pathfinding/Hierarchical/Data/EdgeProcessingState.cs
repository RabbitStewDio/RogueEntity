using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;

public readonly struct EdgeId : IEquatable<EdgeId>
{
    public readonly ushort Id;

    public EdgeId(ushort id)
    {
        Id = id;
    }

    public bool Equals(EdgeId other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is EdgeId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(EdgeId left, EdgeId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(EdgeId left, EdgeId right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"EdgeId({Id})";
    }
}
