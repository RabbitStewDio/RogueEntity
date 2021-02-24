using EnTTSharp.Entities.Attributes;
using MessagePack;
using System;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Players
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public readonly struct PlayerObserverTag : IEquatable<PlayerObserverTag>
    {
        [Key(0)]
        [DataMember(Order=0)]
        public readonly Guid Id;

        [Key(1)]
        [DataMember(Order=1)]
        public readonly PlayerTag ControllingPlayer;

        public readonly bool CanSurvivePlayer; 
        
        [SerializationConstructor]
        public PlayerObserverTag(Guid guid, PlayerTag controllingPlayer, bool canSurvivePlayer)
        {
            ControllingPlayer = controllingPlayer;
            Id = guid;
            CanSurvivePlayer = canSurvivePlayer;
        }

        public static PlayerObserverTag CreateFor(PlayerTag player, bool canSurvivePlayer = false)
        {
            return new PlayerObserverTag(Guid.NewGuid(), player, canSurvivePlayer);
        }
        
        public bool Equals(PlayerObserverTag other)
        {
            return Id.Equals(other.Id) && ControllingPlayer.Equals(other.ControllingPlayer);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerObserverTag other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode() * 397) ^ ControllingPlayer.GetHashCode();
            }
        }

        public static bool operator ==(PlayerObserverTag left, PlayerObserverTag right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerObserverTag left, PlayerObserverTag right)
        {
            return !left.Equals(right);
        }
    }
}
