using System;
using System.Collections.Generic;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public readonly struct DeclaredEntityRoleRecord : IEquatable<DeclaredEntityRoleRecord>
    {
        public readonly Type EntityType;
        readonly List<EntityRole> roles;

        public DeclaredEntityRoleRecord(Type entityType, EntityRole r)
        {
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            this.roles = new List<EntityRole>();
            this.roles.Add(r);
        }

        public ReadOnlyListWrapper<EntityRole> Roles => roles;

        public DeclaredEntityRoleRecord With(EntityRole r)
        {
            if (!roles.Contains(r))
            {
                this.roles.Add(r);
            }

            return this;
        }
        
        public override string ToString()
        {
            return $"{nameof(EntityType)}: {EntityType}, {nameof(Roles)}: {Roles.ToString()}";
        }

        public bool Equals(DeclaredEntityRoleRecord other)
        {
            return EntityType == other.EntityType && CoreExtensions.EqualsList(roles, other.roles);
        }

        public override bool Equals(object obj)
        {
            return obj is DeclaredEntityRoleRecord other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((EntityType != null ? EntityType.GetHashCode() : 0) * 397) ^ (roles != null ? roles.GetHashCode() : 0);
            }
        }

        public static bool operator ==(DeclaredEntityRoleRecord left, DeclaredEntityRoleRecord right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DeclaredEntityRoleRecord left, DeclaredEntityRoleRecord right)
        {
            return !left.Equals(right);
        }
    }
}