using System;

namespace RogueEntity.Api.ItemTraits
{
    public static class EntityRoleInstanceExtensions
    {
        public static EntityRoleInstance Instantiate<TSubjectType>(this EntityRole r)
        {
            return new EntityRoleInstance(r, typeof(TSubjectType));
        }
    }


    public readonly struct EntityRoleInstance : IEquatable<EntityRoleInstance>
    {
        public EntityRoleInstance(EntityRole role, Type entityType)
        {
            this.Role = role;
            this.EntityType = entityType;
        }

        public EntityRole Role { get; }

        public Type EntityType { get; }

        public bool Equals(EntityRoleInstance other)
        {
            return Role.Equals(other.Role) && EntityType == other.EntityType;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityRoleInstance other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Role.GetHashCode() * 397) ^ (EntityType != null ? EntityType.GetHashCode() : 0);
            }
        }

        public static bool operator ==(EntityRoleInstance left, EntityRoleInstance right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityRoleInstance left, EntityRoleInstance right)
        {
            return !left.Equals(right);
        }
    }
}