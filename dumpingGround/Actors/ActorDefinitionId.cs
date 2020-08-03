using System;

namespace ValionRL.Core.Infrastructure.Meta.Actors
{
    public readonly struct ActorDefinitionId : IEquatable<ActorDefinitionId>
    {
        readonly string id;

        public ActorDefinitionId(string id)
        {
            this.id = id;
        }

        public bool Equals(ActorDefinitionId other)
        {
            return id == other.id;
        }

        public override bool Equals(object obj)
        {
            return obj is ActorDefinitionId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (id != null ? id.GetHashCode() : 0);
        }

        public static bool operator ==(ActorDefinitionId left, ActorDefinitionId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ActorDefinitionId left, ActorDefinitionId right)
        {
            return !left.Equals(right);
        }
    }
}