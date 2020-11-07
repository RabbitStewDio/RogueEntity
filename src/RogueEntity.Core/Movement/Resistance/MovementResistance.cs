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
    public readonly struct MovementResistance<TSense> : IEquatable<MovementResistance<TSense>>
    {
        [DataMember(Order = 0)]
        public readonly Percentage BlocksSense;

        [SerializationConstructor]
        public MovementResistance(Percentage blocksSense)
        {
            BlocksSense = blocksSense;
        }

        public MovementResistance(float blocksSense)
        {
            BlocksSense = Percentage.Of(blocksSense);
        }

        public override string ToString()
        {
            return $"{nameof(BlocksSense)}: {BlocksSense}";
        }

        public bool Equals(MovementResistance<TSense> other)
        {
            return BlocksSense.Equals(other.BlocksSense);
        }

        public override bool Equals(object obj)
        {
            return obj is MovementResistance<TSense> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return BlocksSense.GetHashCode();
        }

        public static bool operator ==(MovementResistance<TSense> left, MovementResistance<TSense> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MovementResistance<TSense> left, MovementResistance<TSense> right)
        {
            return !left.Equals(right);
        }
        
        public static MovementResistance<TSense> operator +(MovementResistance<TSense> left, MovementResistance<TSense> right)
        {
            return new MovementResistance<TSense>(left.BlocksSense + right.BlocksSense);
        }


    }
}