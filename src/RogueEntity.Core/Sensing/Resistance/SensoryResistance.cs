using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Resistance
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public readonly struct SensoryResistance<TSense> : IEquatable<SensoryResistance<TSense>>
    {
        public static SensoryResistance<TSense> Blocked = new SensoryResistance<TSense>(Percentage.Full);
        public static SensoryResistance<TSense> Empty = new SensoryResistance<TSense>(Percentage.Empty);
        
        [DataMember(Order = 0)]
        public readonly Percentage BlocksSense;

        [SerializationConstructor]
        public SensoryResistance(Percentage blocksSense)
        {
            BlocksSense = blocksSense;
        }

        public SensoryResistance(float blocksSense)
        {
            BlocksSense = Percentage.Of(blocksSense);
        }

        public override string ToString()
        {
            return $"{nameof(BlocksSense)}: {BlocksSense}";
        }

        public bool Equals(SensoryResistance<TSense> other)
        {
            return BlocksSense.Equals(other.BlocksSense);
        }

        public override bool Equals(object obj)
        {
            return obj is SensoryResistance<TSense> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return BlocksSense.GetHashCode();
        }

        public static bool operator ==(SensoryResistance<TSense> left, SensoryResistance<TSense> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SensoryResistance<TSense> left, SensoryResistance<TSense> right)
        {
            return !left.Equals(right);
        }

        public float ToFloat() => BlocksSense.ToFloat();
        
        public static SensoryResistance<TSense> operator +(SensoryResistance<TSense> left, SensoryResistance<TSense> right)
        {
            return new SensoryResistance<TSense>(left.BlocksSense + right.BlocksSense);
        }


    }
}