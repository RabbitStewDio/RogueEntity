using RogueEntity.Core.Positioning;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Movement.Goals
{
    public readonly struct GoalRecord : IEquatable<GoalRecord>
    {
        public readonly float Strength;
        public readonly Position Position;

        public GoalRecord(float strength, Position position)
        {
            Strength = strength;
            Position = position;
        }

        public bool Equals(GoalRecord other)
        {
            return Strength.Equals(other.Strength) && Position.Equals(other.Position);
        }

        public override bool Equals(object obj)
        {
            return obj is GoalRecord other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Strength.GetHashCode() * 397) ^ Position.GetHashCode();
            }
        }

        public static bool operator ==(GoalRecord left, GoalRecord right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GoalRecord left, GoalRecord right)
        {
            return !left.Equals(right);
        }

        sealed class StrengthRelationalComparer : IComparer<GoalRecord>
        {
            public int Compare(GoalRecord x, GoalRecord y)
            {
                return x.Strength.CompareTo(y.Strength);
            }
        }

        public static IComparer<GoalRecord> StrengthComparer { get; } = new StrengthRelationalComparer();
    }
}
