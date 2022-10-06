using System;

namespace RogueEntity.Api.ItemTraits
{
    public readonly struct WorldEntityTag : IEquatable<WorldEntityTag>
    {
        public readonly string? Tag;

        public WorldEntityTag(string? tag)
        {
            Tag = tag;
        }

        public bool Equals(WorldEntityTag other)
        {
            return Tag == other.Tag;
        }

        public override bool Equals(object obj)
        {
            return obj is WorldEntityTag other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Tag != null ? Tag.GetHashCode() : 0);
        }

        public static bool operator ==(WorldEntityTag left, WorldEntityTag right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WorldEntityTag left, WorldEntityTag right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{nameof(WorldEntityTag)}({Tag})";
        }
    }
}
