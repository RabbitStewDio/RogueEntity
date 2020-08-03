using System;

namespace ValionRL.Core.MapFragments
{
    public readonly struct MapFragmentTagDeclaration : IEquatable<MapFragmentTagDeclaration>
    {
        public static readonly MapFragmentTagDeclaration Empty = default;

        public readonly string GroundTag;
        public readonly string ItemTag;
        public readonly string ActorTag;

        public MapFragmentTagDeclaration(string groundTag, string itemTag, string actorTag)
        {
            GroundTag = groundTag;
            ItemTag = itemTag;
            ActorTag = actorTag;

            if (string.IsNullOrWhiteSpace(GroundTag))
            {
                GroundTag = null;
            }
            if (string.IsNullOrWhiteSpace(ItemTag))
            {
                ItemTag = null;
            }
            if (string.IsNullOrWhiteSpace(ActorTag))
            {
                ActorTag = null;
            }
        }

        public MapFragmentTagDeclaration CombineWith(MapFragmentTagDeclaration other)
        {
            return new MapFragmentTagDeclaration(
                GroundTag ?? other.GroundTag,
                ItemTag ?? other.ItemTag,
                ActorTag ?? other.ActorTag
            );
        }

        public bool Equals(MapFragmentTagDeclaration other)
        {
            return GroundTag == other.GroundTag && ItemTag == other.ItemTag && ActorTag == other.ActorTag;
        }

        public override bool Equals(object obj)
        {
            return obj is MapFragmentTagDeclaration other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (GroundTag != null ? GroundTag.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ItemTag != null ? ItemTag.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ActorTag != null ? ActorTag.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(MapFragmentTagDeclaration left, MapFragmentTagDeclaration right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MapFragmentTagDeclaration left, MapFragmentTagDeclaration right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"({GroundTag}, {ItemTag}, {ActorTag})";
        }
    }
}