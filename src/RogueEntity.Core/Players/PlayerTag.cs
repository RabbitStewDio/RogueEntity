using EnTTSharp.Entities.Attributes;
using MessagePack;
using System;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Players
{
    /// <summary>
    ///   Uniquely identifies a player entity. This data structure is intentionally devoid of
    ///   any other functionality or data. Use custom components if you need more data.
    /// </summary>
    [EntityComponent(EntityConstructor.NonConstructable)]
    [DataContract]
    [MessagePackObject]
    public readonly struct PlayerTag : IEquatable<PlayerTag>
    {
        readonly Guid id;

        public PlayerTag(Guid id)
        {
            this.id = id;
        }

        public bool Equals(PlayerTag other)
        {
            return id.Equals(other.id);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerTag other && Equals(other);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public static bool operator ==(PlayerTag left, PlayerTag right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerTag left, PlayerTag right)
        {
            return !left.Equals(right);
        }
    }
}