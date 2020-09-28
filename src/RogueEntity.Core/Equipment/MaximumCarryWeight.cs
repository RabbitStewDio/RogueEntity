using System;
using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Equipment
{
    public readonly struct MaximumCarryWeight : IEquatable<MaximumCarryWeight>
    {
        public readonly Weight CarryWeight;

        public MaximumCarryWeight(Weight maximumCarryWeight)
        {
            CarryWeight = maximumCarryWeight;
        }

        public bool Equals(MaximumCarryWeight other)
        {
            return CarryWeight.Equals(other.CarryWeight);
        }

        public override bool Equals(object obj)
        {
            return obj is MaximumCarryWeight other && Equals(other);
        }

        public override int GetHashCode()
        {
            return CarryWeight.GetHashCode();
        }

        public static bool operator ==(MaximumCarryWeight left, MaximumCarryWeight right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MaximumCarryWeight left, MaximumCarryWeight right)
        {
            return !left.Equals(right);
        }
    }
    
    
}