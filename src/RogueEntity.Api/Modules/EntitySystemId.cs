using System;

namespace RogueEntity.Api.Modules
{
    public readonly struct EntitySystemId : IEquatable<EntitySystemId>
    {
        public string Id { get; }

        public EntitySystemId(string id)
        {
            Id = id;
        }

        public bool Equals(EntitySystemId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is EntitySystemId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public static bool operator ==(EntitySystemId left, EntitySystemId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntitySystemId left, EntitySystemId right)
        {
            return !left.Equals(right);
        }

        public static implicit operator EntitySystemId(string s)
        {
            return new EntitySystemId(s);
        }

        public override string ToString()
        {
            return Id;
        }
    }
}