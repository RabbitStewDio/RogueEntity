using System;
using System.Runtime.Serialization;
using EnTTSharp.Annotations;
using MessagePack;

namespace RogueEntity.Core.Infrastructure.Meta
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public readonly struct StackCount : IEquatable<StackCount>
    {
        public static readonly StackCount One = new StackCount(1, 1);

        StackCount(ushort count, ushort maximumStackSize)
        {
            this.Count = count;
            this.MaximumStackSize = maximumStackSize;
        }

        [DataMember(Order = 0)]
        [Key(0)]
        public ushort Count { get; }

        [DataMember(Order = 1)]
        [Key(1)]
        public ushort MaximumStackSize { get; }

        public StackCount Take(int count, out StackCount remaining, out int notApplied)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (count > Count)
            {
                notApplied = count - Count;
                remaining = new StackCount(0, MaximumStackSize);
                return new StackCount(Count, MaximumStackSize);
            }

            notApplied = 0;
            remaining = new StackCount((ushort)(Count - count), MaximumStackSize);
            return new StackCount((ushort) count, MaximumStackSize);
        }

        public StackCount WithCount(int count)
        {
            if (count < 0)
            {
                count = 0;
            }
            if (count > MaximumStackSize)
            {
                count = MaximumStackSize;
            }
            return new StackCount((ushort) count, MaximumStackSize);
        }

        public StackCount Add(int count, out int notApplied)
        {
            if (count < 0)
            {
                throw new ArgumentException();
            }

            var total = this.Count + count;
            notApplied = Math.Max(0, total - MaximumStackSize);
            return new StackCount((ushort) Math.Min(total, MaximumStackSize), MaximumStackSize);
        }

        public bool Merge(in StackCount stackSizeNew, out StackCount result)
        {
            if (Count + stackSizeNew.Count <= MaximumStackSize)
            {
                result = new StackCount((ushort) (Count + stackSizeNew.Count), MaximumStackSize);
                return true;
            }

            result = default;
            return false;
        }

        public static StackCount Of(ushort maxSize)
        {
            return new StackCount(0, maxSize);
        }

        public bool Equals(StackCount other)
        {
            return Count == other.Count && MaximumStackSize == other.MaximumStackSize;
        }

        public override bool Equals(object obj)
        {
            return obj is StackCount other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Count.GetHashCode() * 397) ^ MaximumStackSize.GetHashCode();
            }
        }

        public static bool operator ==(StackCount left, StackCount right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StackCount left, StackCount right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"Stack({nameof(Count)}: {Count} / {MaximumStackSize})";
        }
    }
}