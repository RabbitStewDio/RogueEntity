using System;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Meta.StatusEffects
{
    public class StatusFlag : IEquatable<StatusFlag>
    {
        public readonly int LinearIndex;
        public StatusFlagRegistry Owner { get; }

        internal StatusFlag(StatusFlagRegistry owner, string id, int linearIndex)
        {
            this.TypedProperties = new TypedRuleProperties();
            this.Owner = owner;
            this.Id = id;
            this.LinearIndex = linearIndex;
            this.Properties = new RuleProperties();
        }

        public TypedRuleProperties TypedProperties { get; }
        public RuleProperties Properties { get; }

        public bool Equals(StatusFlag other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Owner == other.Owner && Id == other.Id && LinearIndex == other.LinearIndex;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((StatusFlag)obj);
        }

        public override int GetHashCode()
        {
            return LinearIndex;
        }

        public static bool operator ==(StatusFlag left, StatusFlag right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(StatusFlag left, StatusFlag right)
        {
            return !Equals(left, right);
        }

        public string Id { get; }

        public override string ToString()
        {
            return $"[{LinearIndex}] {Id}";
        }
    }
}