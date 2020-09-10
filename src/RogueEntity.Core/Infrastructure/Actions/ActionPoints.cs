using System;
using System.Runtime.Serialization;
using EnTTSharp.Annotations;
using EnTTSharp.Entities.Attributes;
using MessagePack;

namespace RogueEntity.Core.Infrastructure.Actions
{
    /// <summary>
    ///   Represents an action point debt. Action points are needed to
    ///   perform any action. All actions can be performed for as long
    ///   as the character has a positive AP balance. 
    /// </summary>
    [EntityComponent]
    [DataContract]
    [MessagePackObject]
    public readonly struct ActionPoints : IComparable<ActionPoints>, IComparable, IEquatable<ActionPoints>
    {
        public static readonly ActionPoints Zero = new ActionPoints();

        [DataMember(Order = 0)]
        [Key(0)]
        readonly int balance;

        [SerializationConstructor]
        ActionPoints(int balance)
        {
            this.balance = balance;
        }

        public bool Equals(ActionPoints other)
        {
            return balance == other.balance;
        }

        public override bool Equals(object obj)
        {
            return obj is ActionPoints other && Equals(other);
        }

        public override int GetHashCode()
        {
            return balance;
        }

        public static bool operator ==(ActionPoints left, ActionPoints right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ActionPoints left, ActionPoints right)
        {
            return !left.Equals(right);
        }

        public int CompareTo(ActionPoints other)
        {
            return balance.CompareTo(other.balance);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            return obj is ActionPoints other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ActionPoints)}");
        }

        public static bool operator <(ActionPoints left, ActionPoints right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(ActionPoints left, ActionPoints right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(ActionPoints left, ActionPoints right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(ActionPoints left, ActionPoints right)
        {
            return left.CompareTo(right) >= 0;
        }

        // public static ActionPoints operator +(ActionPoints left, int right)
        // {
        //     return new ActionPoints(left.balance + right);
        // }
        //
        // public static ActionPoints operator -(ActionPoints left, int right)
        // {
        //     return new ActionPoints(left.balance - right);
        // }

        public static ActionPoints From(int v)
        {
            return new ActionPoints(v);
        }

        public bool TryRecover(int points, out ActionPoints p)
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
            return $"ActionPoints({Balance})";
        }

        public ActionPoints Spend(int actionCost)
        {
            if (actionCost < 0)
            {
                throw new ArgumentException();
            }

            return new ActionPoints(balance - actionCost);
        }
    }
}