using System;

namespace RogueEntity.Core.Infrastructure.Actions
{
    /// <summary>
    ///   Represents an action point debt. Action points are needed to
    ///   perform any action. All actions can be performed for as long
    ///   as the character has a positive AP balance. 
    /// </summary>
    public readonly struct MovementPoints : IComparable<MovementPoints>, IComparable, IEquatable<MovementPoints>
    {
        public static readonly MovementPoints Zero = new MovementPoints();

        readonly int balance;

        MovementPoints(int balance)
        {
            this.balance = balance;
        }

        public bool Equals(MovementPoints other)
        {
            return balance == other.balance;
        }

        public override bool Equals(object obj)
        {
            return obj is MovementPoints other && Equals(other);
        }

        public override int GetHashCode()
        {
            return balance;
        }

        public static bool operator ==(MovementPoints left, MovementPoints right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MovementPoints left, MovementPoints right)
        {
            return !left.Equals(right);
        }

        public int CompareTo(MovementPoints other)
        {
            return balance.CompareTo(other.balance);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            return obj is MovementPoints other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(MovementPoints)}");
        }

        public static bool operator <(MovementPoints left, MovementPoints right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(MovementPoints left, MovementPoints right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(MovementPoints left, MovementPoints right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(MovementPoints left, MovementPoints right)
        {
            return left.CompareTo(right) >= 0;
        }

        // public static MovementPoints operator +(MovementPoints left, int right)
        // {
        //     return new MovementPoints(Math.Min(0, left.balance + right));
        // }
        //
        // public static MovementPoints operator -(MovementPoints left, int right)
        // {
        //     return new MovementPoints(Math.Min(0, left.balance - right));
        // }

        public static MovementPoints From(int v)
        {
            return new MovementPoints(v);
        }

        public bool TryRecover(int points, out MovementPoints p)
        {
            if (points < 0 || balance >= 0)
            {
                p = this;
                return false;
            }

            p = From(balance + points);
            return true;
        }

        public bool CanPerformActions() => balance >= 0;

        public int Balance => balance;

        public override string ToString()
        {
            return $"MovementPoints({Balance})";
        }

        public MovementPoints Spend(int actionCost)
        {
            if (actionCost < 0)
            {
                throw new ArgumentException();
            }

            return new MovementPoints(balance - actionCost);
        }
    }
}