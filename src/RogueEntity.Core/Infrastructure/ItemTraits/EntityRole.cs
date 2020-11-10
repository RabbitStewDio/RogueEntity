using System;

namespace RogueEntity.Core.Infrastructure.ItemTraits
{
    public readonly struct EntityRole : IEquatable<EntityRole>
    {
        public readonly string Id;

        public EntityRole(string id)
        {
            Id = id;
        }

        public bool Equals(EntityRole other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityRole other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public static bool operator ==(EntityRole left, EntityRole right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityRole left, EntityRole right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"Role({Id})";
        }
    }
}