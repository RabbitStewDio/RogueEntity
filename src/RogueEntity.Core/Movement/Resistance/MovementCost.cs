using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Movement.Resistance
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public readonly struct MovementCost<TSense> : IEquatable<MovementCost<TSense>>
    {
        [DataMember(Order = 0)]
        public readonly Percentage BlocksSense;

        [SerializationConstructor]
        public MovementCost(Percentage blocksSense)
        {
            BlocksSense = blocksSense;
        }

        public MovementCost(float blocksSense)
        {
            BlocksSense = Percentage.Of(blocksSense);
        }

        public override string ToString()
        {
            return $"{nameof(BlocksSense)}: {BlocksSense}";
        }

        public bool Equals(MovementCost<TSense> other)
        {
            return BlocksSense.Equals(other.BlocksSense);
        }

        public override bool Equals(object obj)
        {
            return obj is MovementCost<TSense> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return BlocksSense.GetHashCode();
        }

        public static bool operator ==(MovementCost<TSense> left, MovementCost<TSense> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MovementCost<TSense> left, MovementCost<TSense> right)
        {
            return !left.Equals(right);
        }
        
        public static MovementCost<TSense> operator +(MovementCost<TSense> left, MovementCost<TSense> right)
        {
            return new MovementCost<TSense>(left.BlocksSense + right.BlocksSense);
        }


    }
}