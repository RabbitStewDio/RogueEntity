using System;

namespace RogueEntity.Api.ItemTraits
{
    public readonly struct ItemTraitId : IEquatable<ItemTraitId>
    {
        public readonly string Id;

        public ItemTraitId(string id)
        {
            Id = id;
        }

        public static implicit operator ItemTraitId(string id)
        {
            return new ItemTraitId(id);
        }

        public bool Equals(ItemTraitId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is ItemTraitId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public static bool operator ==(ItemTraitId left, ItemTraitId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ItemTraitId left, ItemTraitId right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return Id;
        }

    }
}