using System;

namespace RogueEntity.Core.Movement.Maps
{
    /// <summary>
    ///   Combines the available movement options of all 4 movement modes into a single
    ///   32-bit value.
    /// </summary>
    public readonly struct MovementAllowedProperties : IEquatable<MovementAllowedProperties>
    {
        public readonly MovementAllowedData Walking;
        public readonly MovementAllowedData Flying;
        public readonly MovementAllowedData Ethereal;
        public readonly MovementAllowedData Swimming;

        public MovementAllowedProperties(MovementAllowedData walking, MovementAllowedData flying, MovementAllowedData ethereal, MovementAllowedData swimming)
        {
            Walking = walking;
            Flying = flying;
            Ethereal = ethereal;
            Swimming = swimming;
        }

        public bool Equals(MovementAllowedProperties other)
        {
            return Walking.Equals(other.Walking) && Flying.Equals(other.Flying) && Ethereal.Equals(other.Ethereal) && Swimming.Equals(other.Swimming);
        }

        public override bool Equals(object obj)
        {
            return obj is MovementAllowedProperties other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Walking.GetHashCode();
                hashCode = (hashCode * 397) ^ Flying.GetHashCode();
                hashCode = (hashCode * 397) ^ Ethereal.GetHashCode();
                hashCode = (hashCode * 397) ^ Swimming.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(MovementAllowedProperties left, MovementAllowedProperties right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MovementAllowedProperties left, MovementAllowedProperties right)
        {
            return !left.Equals(right);
        }
    }
}