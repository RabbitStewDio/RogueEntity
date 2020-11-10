using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Movement.CostModifier
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public readonly struct MovementCostModifier<TSense> : IEquatable<MovementCostModifier<TSense>>
    {
        [DataMember(Order = 0)]
        public readonly ushort CostModifier;

        [SerializationConstructor]
        public MovementCostModifier(ushort costModifier)
        {
            CostModifier = costModifier;
        }

        public MovementCostModifier(float blocksSense)
        {
            CostModifier = (ushort) (blocksSense * 100).Clamp(0, ushort.MaxValue);
        }

        public float Value => CostModifier / 100f;
        
        public override string ToString()
        {
            return $"{nameof(CostModifier)}: {CostModifier}";
        }

        public bool Equals(MovementCostModifier<TSense> other)
        {
            return CostModifier.Equals(other.CostModifier);
        }

        public override bool Equals(object obj)
        {
            return obj is MovementCostModifier<TSense> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return CostModifier.GetHashCode();
        }

        public static bool operator ==(MovementCostModifier<TSense> left, MovementCostModifier<TSense> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MovementCostModifier<TSense> left, MovementCostModifier<TSense> right)
        {
            return !left.Equals(right);
        }

        public static implicit operator float(MovementCostModifier<TSense> v) => v.Value;
        
        public static MovementCostModifier<TSense> operator +(MovementCostModifier<TSense> left, MovementCostModifier<TSense> right)
        {
            return new MovementCostModifier<TSense>(left.CostModifier + right.CostModifier);
        }


    }
}