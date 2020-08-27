using System;

namespace RogueEntity.Core.Meta.Items
{
    public readonly struct ItemDeclarationId : IEquatable<ItemDeclarationId>
    {
        readonly string id;

        public ItemDeclarationId(string id)
        {
            this.id = id;
        }

        public bool Equals(ItemDeclarationId other)
        {
            return id == other.id;
        }

        public override bool Equals(object obj)
        {
            return obj is ItemDeclarationId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (id != null ? id.GetHashCode() : 0);
        }

        public static bool operator ==(ItemDeclarationId left, ItemDeclarationId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ItemDeclarationId left, ItemDeclarationId right)
        {
            return !left.Equals(right);
        }
    }
}