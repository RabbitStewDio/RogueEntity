using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Infrastructure.Meta.Items
{
    public abstract class ItemDeclaration<TContext> : IEquatable<ItemDeclaration<TContext>>, IItemDeclaration
    {
        public string Tag { get; }
        public ItemDeclarationId Id { get; }

        protected ItemDeclaration(ItemDeclarationId id,
                    string tag)
        {
            Id = id;
            Tag = tag;
        }

        public abstract bool TryQuery<TTrait>(out TTrait t) where TTrait : IItemTrait;

        public abstract List<TTrait> QueryAll<TTrait>(List<TTrait> cache = null) where TTrait : IItemTrait;

        public bool Equals(ItemDeclaration<TContext> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((ItemDeclaration<TContext>) obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public static bool operator ==(ItemDeclaration<TContext> left, ItemDeclaration<TContext> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ItemDeclaration<TContext> left, ItemDeclaration<TContext> right)
        {
            return !Equals(left, right);
        }
    }
}