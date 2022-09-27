using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;

namespace RogueEntity.Api.Modules
{
    [Obsolete]
    public interface IModuleEntityActivatorCallback
    {
        public void ActivateEntity<TEntity>(DeclaredEntityRoleRecord record) where TEntity : struct, IEntityKey;
    }

    [Obsolete]
    public readonly struct DeclaredEntityRoleRecord : IEquatable<DeclaredEntityRoleRecord>
    {
        public readonly Type EntityType;
        readonly List<EntityRole> roles;
        readonly Action<IModuleEntityActivatorCallback, DeclaredEntityRoleRecord> activatorFunction;

        public DeclaredEntityRoleRecord(Type entityType, EntityRole r, Action<IModuleEntityActivatorCallback, DeclaredEntityRoleRecord> activatorFunction)
        {
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            this.activatorFunction = activatorFunction;
            this.roles = new List<EntityRole>();
            this.roles.Add(r);
        }

        public void Activate(IModuleEntityActivatorCallback cb) => activatorFunction.Invoke(cb, this);
        
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
            return EntityType == other.EntityType && ApiExtensions.EqualsList(roles, other.roles);
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