using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Movement.CostModifier
{
    /// <summary>
    ///   Encodes a movement cost relative to unencumbered movement. This can be considered the inverse of the
    ///   actors velocity to traverse a tile. A relative cost modifier of zero indicates that a tile is blocked.   
    ///
    ///   A relative movement cost value of 2 means the actor needs twice as much energy or time to traverse a
    ///   unit of space. A relative movement cost value of 0.5 indicates that the actor moves twice as fast as
    ///   normal.  
    /// </summary>
    /// <typeparam name="TMovementMode"></typeparam>
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public readonly struct RelativeMovementCostModifier<TMovementMode> : IEquatable<RelativeMovementCostModifier<TMovementMode>>
    {
        public static readonly RelativeMovementCostModifier<TMovementMode> Blocked = new RelativeMovementCostModifier<TMovementMode>(0);
        public static readonly RelativeMovementCostModifier<TMovementMode> Unchanged = new RelativeMovementCostModifier<TMovementMode>(1f);
        
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly ushort CostModifier;

        [SerializationConstructor]
        public RelativeMovementCostModifier(ushort costModifier)
        {
            CostModifier = costModifier;
        }

        public RelativeMovementCostModifier(float blocksSense)
        {
            CostModifier = (ushort) (blocksSense * 100).Clamp(0, ushort.MaxValue);
        }

        [IgnoreDataMember]
        [IgnoreMember]
        public float Value => CostModifier / 100f;
        
        public override string ToString()
        {
            return $"{nameof(CostModifier)}: {CostModifier}";
        }

        public bool Equals(RelativeMovementCostModifier<TMovementMode> other)
        {
            return CostModifier.Equals(other.CostModifier);
        }

        public override bool Equals(object obj)
        {
            return obj is RelativeMovementCostModifier<TMovementMode> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return CostModifier.GetHashCode();
        }

        public static bool operator ==(RelativeMovementCostModifier<TMovementMode> left, RelativeMovementCostModifier<TMovementMode> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RelativeMovementCostModifier<TMovementMode> left, RelativeMovementCostModifier<TMovementMode> right)
        {
            return !left.Equals(right);
        }

        public static implicit operator float(RelativeMovementCostModifier<TMovementMode> v) => v.Value;
        public static implicit operator RelativeMovementCostModifier<TMovementMode>(float v) => new RelativeMovementCostModifier<TMovementMode>(v);
        public static implicit operator RelativeMovementCostModifier<TMovementMode>(Percentage v) => new RelativeMovementCostModifier<TMovementMode>(v);
        
        public static RelativeMovementCostModifier<TMovementMode> operator *(RelativeMovementCostModifier<TMovementMode> left, RelativeMovementCostModifier<TMovementMode> right)
        {
            return new RelativeMovementCostModifier<TMovementMode>(left.CostModifier + right.CostModifier);
        }


    }
}