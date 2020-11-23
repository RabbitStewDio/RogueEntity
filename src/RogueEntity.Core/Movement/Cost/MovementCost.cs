using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Movement.Cost
{
    /// <summary>
    ///   Describes a units ability to move as the energy it takes to traverse a unit of distance.
    ///   The actual interpretation of this value is beyond the scope of this module, but the
    ///   values given should be consistent across movement modes.
    ///
    ///   Pathfinding will attempt to minimize movement costs for a given path.
    ///
    ///   Thus if a unit swims twice as fast as it walks, the  unit should define a movement cost
    ///   for walking as twice the amount given for walking. Movement cost can be directly modelled
    ///   as action point costs; or inversely as velocity. 
    /// </summary>
    [EntityComponent]
    [MessagePackObject]
    [DataContract]
    public readonly struct MovementCost : IComparable<MovementCost>, IComparable, IEquatable<MovementCost>
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly IMovementMode MovementMode;

        [DataMember(Order = 1)]
        [Key(1)]
        public readonly DistanceCalculation MovementStyle;

        /// <summary>
        ///   A movement cost indicator as fixed point number. (16/16 split) 
        /// </summary>
        [DataMember(Order = 2)]
        [Key(2)]
        public readonly float Cost;

        /// <summary>
        ///   A tie-breaker value indicating which movement mode should be preferred if
        ///   both movement costs are equal. If that does not solve the problem, we fall
        ///   back to brute-force by sorting by MovementMode classname.
        /// </summary>
        [DataMember(Order = 3)]
        [Key(3)]
        public readonly int Preference;

        [SerializationConstructor]
        public MovementCost(IMovementMode movementMode, DistanceCalculation movementStyle, float cost, int preference = 0)
        {
            MovementStyle = movementStyle;
            MovementMode = movementMode;
            Cost = cost;
            Preference = preference;
        }

        public int CompareTo(MovementCost other)
        {
            var costComparison = Cost.CompareTo(other.Cost);
            if (costComparison != 0)
            {
                return costComparison;
            }

            var preferenceComparison = Preference.CompareTo(other.Preference);
            if (preferenceComparison != 0)
            {
                return preferenceComparison;
            }

            return string.CompareOrdinal(MovementModeAsText, other.MovementModeAsText);
        }

        [IgnoreDataMember]
        [IgnoreMember]
        string MovementModeAsText => MovementMode == null ? "" : MovementMode.GetType().Name;

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            return obj is MovementCost other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(MovementCost)}");
        }

        public static bool operator <(MovementCost left, MovementCost right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(MovementCost left, MovementCost right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(MovementCost left, MovementCost right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(MovementCost left, MovementCost right)
        {
            return left.CompareTo(right) >= 0;
        }

        public bool Equals(MovementCost other)
        {
            return MovementStyle == other.MovementStyle && Equals(MovementMode, other.MovementMode) && Cost.Equals(other.Cost) && Preference == other.Preference;
        }

        public override bool Equals(object obj)
        {
            return obj is MovementCost other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) MovementStyle;
                hashCode = (hashCode * 397) ^ (MovementMode != null ? MovementMode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Cost.GetHashCode();
                hashCode = (hashCode * 397) ^ Preference;
                return hashCode;
            }
        }

        public static bool operator ==(MovementCost left, MovementCost right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MovementCost left, MovementCost right)
        {
            return !left.Equals(right);
        }
    }
}