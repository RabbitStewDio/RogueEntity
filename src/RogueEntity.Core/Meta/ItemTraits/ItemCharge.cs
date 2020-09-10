using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;

namespace RogueEntity.Core.Meta.ItemTraits
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public readonly struct ItemCharge : IEquatable<ItemCharge>, IComparable<ItemCharge>, IComparable
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly ushort Count;
        [DataMember(Order = 1)]
        [Key(1)]
        public readonly ushort MaximumCharge;

        [SerializationConstructor]
        public ItemCharge(ushort charge, ushort maxCharge)
        {
            this.MaximumCharge = maxCharge;
            this.Count = Math.Min(charge, MaximumCharge);
        }

        [IgnoreMember]
        public bool Empty => Count == 0;

        [IgnoreMember]
        public bool Unlimited => Count == ushort.MaxValue && MaximumCharge == ushort.MaxValue;

        public ItemCharge WithCount(int count)
        {
            if (count < 0)
            {
                count = 0;
            }
            if (count > MaximumCharge)
            {
                count = MaximumCharge;
            }
            return new ItemCharge((ushort) count, MaximumCharge);
        }

        public bool TryConsume(int charge, out ItemCharge c)
        {
            if (Unlimited)
            {
                c = this;
                return true;
            }

            if (charge < 0)
            {
                throw new ArgumentException();
            }

            if (charge == 0 || charge > Count)
            {
                c = this;
                return false;
            }

            ushort nextCharge = (ushort)(Count - charge);
            c = new ItemCharge(nextCharge, MaximumCharge);
            return true;
        }

        public bool Equals(ItemCharge other)
        {
            return Count == other.Count && MaximumCharge == other.MaximumCharge;
        }

        public override bool Equals(object obj)
        {
            return obj is ItemCharge other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Count.GetHashCode() * 397) ^ MaximumCharge.GetHashCode();
            }
        }

        public static bool operator ==(ItemCharge left, ItemCharge right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ItemCharge left, ItemCharge right)
        {
            return !left.Equals(right);
        }

        public int CompareTo(ItemCharge other)
        {
            var countComparison = Count.CompareTo(other.Count);
            if (countComparison != 0)
            {
                return countComparison;
            }

            return MaximumCharge.CompareTo(other.MaximumCharge);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            return obj is ItemCharge other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ItemCharge)}");
        }

        public static bool operator <(ItemCharge left, ItemCharge right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(ItemCharge left, ItemCharge right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(ItemCharge left, ItemCharge right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(ItemCharge left, ItemCharge right)
        {
            return left.CompareTo(right) >= 0;
        }

        public override string ToString()
        {
            return $"Charge({Count} of {MaximumCharge})";
        }
    }
}