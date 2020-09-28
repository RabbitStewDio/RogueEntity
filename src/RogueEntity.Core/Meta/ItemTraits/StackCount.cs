using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;

namespace RogueEntity.Core.Meta.ItemTraits
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

        /// <summary>
        ///   Removes the given number of elements from the item stack. The given stack
        ///   will be split between a stack of items taken (returned as return value)
        ///   and the stack of items remaining in place. If more items are requested
        ///   than available in the stack, itemsLeftToBeRemoved will contain that number
        ///   of items that should be taken from other stacks elsewhere.
        ///
        ///   The out parameter remainingItemsInPlace holds the stack of items left.
        ///   This stack can be empty (zero count) if all items have been removed.
        ///   In  that case you should perform some clean-up to remove the item reference.
        /// </summary>
        /// <param name="itemsToBeRemoved"></param>
        /// <param name="remainingItemsInPlace"></param>
        /// <param name="itemsLeftToBeRemoved"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public StackCount Take(int itemsToBeRemoved, out StackCount remainingItemsInPlace, out int itemsLeftToBeRemoved)
        {
            if (itemsToBeRemoved < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (itemsToBeRemoved > Count)
            {
                itemsLeftToBeRemoved = itemsToBeRemoved - Count;
                remainingItemsInPlace = new StackCount(0, MaximumStackSize);
                return new StackCount(Count, MaximumStackSize);
            }

            itemsLeftToBeRemoved = 0;
            remainingItemsInPlace = new StackCount((ushort)(Count - itemsToBeRemoved), MaximumStackSize);
            return new StackCount((ushort) itemsToBeRemoved, MaximumStackSize);
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

        public bool Merge(in StackCount additionalStack, out StackCount result)
        {
            if (Count + additionalStack.Count <= MaximumStackSize)
            {
                result = new StackCount((ushort) (Count + additionalStack.Count), MaximumStackSize);
                return true;
            }

            result = default;
            return false;
        }

        public static StackCount Of(ushort maxSize)
        {
            return new StackCount(0, maxSize);
        }

        public static StackCount Of(ushort count, ushort maxSize)
        {
            if (count > maxSize)
            {
                count = maxSize;
            }
            return new StackCount(count, maxSize);
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