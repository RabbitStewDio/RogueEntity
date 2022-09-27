using System;
using System.Collections.Generic;
using RogueEntity.Api.Utils;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Api.ItemTraits
{
    public abstract class ItemDeclaration : IEquatable<ItemDeclaration>, IItemDeclaration
    {
        public WorldEntityTag Tag { get; }
        public ItemDeclarationId Id { get; }

        protected ItemDeclaration(ItemDeclarationId id,
                                  WorldEntityTag tag)
        {
            Id = id;
            Tag = tag;
        }

        public abstract bool TryQuery<TTrait>([MaybeNullWhen(false)] out TTrait t)
            where TTrait : IItemTrait;

        public abstract BufferList<TTrait> QueryAll<TTrait>(BufferList<TTrait>? cache = null)
            where TTrait : IItemTrait;

        public bool Equals(ItemDeclaration other)
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

            return Equals((ItemDeclaration)obj);
        }

        public override int GetHashCode()
        {
            return (Id.GetHashCode());
        }

        public static bool operator ==(ItemDeclaration left, ItemDeclaration right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ItemDeclaration left, ItemDeclaration right)
        {
            return !Equals(left, right);
        }

        public IEnumerable<(ItemTraitId traitId, EntityRoleInstance role)> GetEntityRoles()
        {
            foreach (var t in QueryAll<IItemTrait>())
            {
                foreach (var r in t.GetEntityRoles())
                {
                    yield return ((t.Id, r));
                }
            }
        }

        public IEnumerable<(ItemTraitId traitId, EntityRelationInstance relation)> GetEntityRelations()
        {
            foreach (var t in QueryAll<IItemTrait>())
            {
                foreach (var r in t.GetEntityRelations())
                {
                    yield return ((t.Id, r));
                }
            }
        }
    }
}
