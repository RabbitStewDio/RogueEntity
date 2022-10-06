using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical
{
    /// <summary>
    ///    A strongly typed identifier of a walkable zone inside a pathfinder region.
    /// </summary>
    public readonly struct TraversableZoneId : IEquatable<TraversableZoneId>
    {
        public static readonly TraversableZoneId Empty = new TraversableZoneId();
        public readonly ushort Id;

        public TraversableZoneId(ushort id)
        {
            this.Id = id;
        }

        public bool Equals(TraversableZoneId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            return obj is TraversableZoneId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(TraversableZoneId left, TraversableZoneId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TraversableZoneId left, TraversableZoneId right)
        {
            return !left.Equals(right);
        }
    }
}