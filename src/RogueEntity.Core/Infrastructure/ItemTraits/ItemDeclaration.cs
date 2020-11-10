using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Infrastructure.ItemTraits
{
    public abstract class ItemDeclaration<TGameContext> : IEquatable<ItemDeclaration<TGameContext>>, IItemDeclaration
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

        public bool Equals(ItemDeclaration<TGameContext> other)
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

            return Equals((ItemDeclaration<TGameContext>) obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public static bool operator ==(ItemDeclaration<TGameContext> left, ItemDeclaration<TGameContext> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ItemDeclaration<TGameContext> left, ItemDeclaration<TGameContext> right)
        {
            return !Equals(left, right);
        }

        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            var roles = new List<EntityRoleInstance>();
            foreach (var t in QueryAll<IItemTrait>())
            {
                roles.AddRange(t.GetEntityRoles());
            }

            return roles;
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            var roles = new List<EntityRelationInstance>();
            foreach (var t in QueryAll<IItemTrait>())
            {
                roles.AddRange(t.GetEntityRelations());
            }

            return roles;
        }
    }
}