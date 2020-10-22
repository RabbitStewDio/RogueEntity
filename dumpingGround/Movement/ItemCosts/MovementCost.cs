using System;
using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Movement.ItemCosts
{
    /// <summary>
    ///   MovementCosts are a relative cost applied to a character's base movement time.
    ///   Each character consumes movement points when moving around the map. Items and
    ///   ground conditions can modify this cost.
    ///
    ///   Movement cost is measured as percentage value in the range of 0 to 500%, with
    ///   any value above 500% considered impassable.
    ///
    ///   This struct encodes movement cost as a percentage value using a byte as data
    ///   representation. Using bytes means we can efficiently store all movement conditions
    ///   in a easily blittable cache (<see cref="MovementCostMap"/>).
    ///
    ///   Movement cost can cover a range from 0 to 500%, encoded as relative measure where
    ///   the byte value 50 represents 100%. The byte value of 255 is interpreted as 'blocked'
    ///   movement.
    /// </summary>
    [DataContract]
    [Serializable]
    [MessagePackObject]
    public readonly struct MovementCost : IEquatable<MovementCost>, IComparable<MovementCost>, IComparable
    {
        public static readonly MovementCost Blocked = new MovementCost(byte.MaxValue);
        public static readonly MovementCost Normal = new MovementCost(50);
        public static readonly MovementCost Free = new MovementCost(0);

        [DataMember(Order = 0)]
        [Key(0)]
        readonly byte data;

        [SerializationConstructor]
        public MovementCost(byte data)
        {
            if (data > 250)
            {
                this.data = byte.MaxValue;
            }
            else
            {
                this.data = data;
            }
        }

        public MovementCost(float movementCostFactor)
        {
            if (movementCostFactor <= 0)
            {
                this.data = 0;
            }
            else if (movementCostFactor > 5)
            {
                this.data = byte.MaxValue;
            }
            else
            {
                this.data = (byte)(movementCostFactor * 50);
            }
        }

        public bool CanMove(out byte cost)
        {
            cost = data;
            return cost != byte.MaxValue;
        }

        public MovementCost Combine(MovementCost other)
        {
            return new MovementCost(Math.Max(data, other.data));
        }

        public MovementCost Reduce(MovementCost other)
        {
            return new MovementCost(Math.Min(data, other.data));
        }

        public MovementCost Apply(float other)
        {
            if (data == 255)
            {
                return Blocked;
            }

            var effectiveCost = (byte) (data * other);
            return new MovementCost(effectiveCost);
        }

        public bool TryApply(out float effectiveCost)
        {
            if (data == 255)
            {
                effectiveCost = float.MaxValue;
                return false;
            }

            effectiveCost = data / 50f;
            return true;
        }

        public bool TryApply(float baseCost, out float effectiveCost)
        {
            if (data == 255)
            {
                effectiveCost = float.MaxValue;
                return false;
            }

            effectiveCost = baseCost * data / 50f;
            return true;
        }

        public float Cost => data > 250 ? float.MaxValue : data / 50f;

        public byte RawCost => data;

        public override string ToString()
        {
            if (data == byte.MaxValue)
            {
                return "Blocked";
            }
            
            return $"{Cost:P}";
        }

        public bool Equals(MovementCost other) => data == other.data;

        public override bool Equals(object obj) => obj is MovementCost other && Equals(other);

        public override int GetHashCode() => data.GetHashCode();

        public static bool operator ==(MovementCost left, MovementCost right) => left.Equals(right);

        public static bool operator !=(MovementCost left, MovementCost right) => !left.Equals(right);

        public int CompareTo(MovementCost other) => data.CompareTo(other.data);

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            return obj is MovementCost other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(MovementCost)}");
        }

        public static bool operator <(MovementCost left, MovementCost right) => left.CompareTo(right) < 0;

        public static bool operator >(MovementCost left, MovementCost right) => left.CompareTo(right) > 0;

        public static bool operator <=(MovementCost left, MovementCost right) => left.CompareTo(right) <= 0;

        public static bool operator >=(MovementCost left, MovementCost right) => left.CompareTo(right) >= 0;
    }
}