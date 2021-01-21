using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public readonly struct StackTakeResult : IEquatable<StackTakeResult>
    {
        public readonly StackCount ItemsTakenFromStack;
        public readonly int ItemsNotAvailableInStack;
        public readonly Optional<StackCount> ItemsLeftInStack;

        public StackTakeResult(StackCount itemsTakenFromStack, 
                               int itemsNotAvailableInStack, 
                               Optional<StackCount> itemsLeftInStack)
        {
            ItemsTakenFromStack = itemsTakenFromStack;
            ItemsNotAvailableInStack = itemsNotAvailableInStack;
            ItemsLeftInStack = itemsLeftInStack;
        }

        public bool NotEnoughItemsInStack => ItemsNotAvailableInStack > 0;

        public bool Equals(StackTakeResult other)
        {
            return ItemsTakenFromStack.Equals(other.ItemsTakenFromStack) && ItemsNotAvailableInStack == other.ItemsNotAvailableInStack && ItemsLeftInStack.Equals(other.ItemsLeftInStack);
        }

        public override bool Equals(object obj)
        {
            return obj is StackTakeResult other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ItemsTakenFromStack.GetHashCode();
                hashCode = (hashCode * 397) ^ ItemsNotAvailableInStack;
                hashCode = (hashCode * 397) ^ ItemsLeftInStack.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(StackTakeResult left, StackTakeResult right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StackTakeResult left, StackTakeResult right)
        {
            return !left.Equals(right);
        }
    }
    
    
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public readonly struct StackCount : IEquatable<StackCount>
    {
        [DataMember(Order = 0)]
        [Key(0)]
        internal readonly ushort CountRaw;
        
        [DataMember(Order = 1)]
        [Key(1)]
        readonly ushort maximumStackSize;
        
        public static readonly StackCount One = new StackCount(1, 1);

        [SerializationConstructor]
        StackCount(ushort countRaw, ushort maximumStackSize)
        {
            this.CountRaw = countRaw;
            this.maximumStackSize = maximumStackSize;
        }

        StackCount(int count, int maximumStackSize)
        {
            if (count <= 0) throw new ArgumentException();
            if (count > maximumStackSize) throw new ArgumentException();
            
            this.CountRaw = (count - 1).ClampToUnsignedShort();
            this.maximumStackSize = (maximumStackSize - 1).ClampToUnsignedShort();
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public int Count => CountRaw + 1;

        [IgnoreMember]
        [IgnoreDataMember]
        public int MaximumStackSize => maximumStackSize + 1;

        [IgnoreMember]
        [IgnoreDataMember]
        public bool IsFullStack => CountRaw == maximumStackSize;
        
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
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public StackTakeResult Take(int itemsToBeRemoved)
        {
            if (itemsToBeRemoved <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(itemsToBeRemoved), "itemsToBeRemoved must be a positive, non-zero integer");
            }

            if (itemsToBeRemoved > Count)
            {
                var remainingItems = itemsToBeRemoved - Count;
                return new StackTakeResult(new StackCount(Count, MaximumStackSize), remainingItems, default);
            }

            if (itemsToBeRemoved == Count)
            {
                return new StackTakeResult(new StackCount(itemsToBeRemoved, MaximumStackSize), 0, default);
            }
            
            var itemsLeftInStack = new StackCount(Count - itemsToBeRemoved, MaximumStackSize);
            return new StackTakeResult(new StackCount(itemsToBeRemoved, MaximumStackSize), 0, itemsLeftInStack);
        }

        public StackCount WithCount(int newCount)
        {
            if (newCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(newCount), $"Stack size cannot be less than 1");
            }
            if (newCount > MaximumStackSize)
            {
                throw new ArgumentOutOfRangeException(nameof(newCount), $"Stack size cannot exceed maximum stack size of {MaximumStackSize}");
            }
            return new StackCount(newCount, MaximumStackSize);
        }

        public StackCount Add(int additionalItems, out int notApplied)
        {
            if (additionalItems < 0)
            {
                throw new ArgumentException();
            }

            var total = this.Count + additionalItems;
            notApplied = Math.Max(0, total - MaximumStackSize);
            return new StackCount(Math.Min(total, MaximumStackSize), MaximumStackSize);
        }

        public bool Merge(in StackCount additionalStack, out StackCount result)
        {
            if (Count + additionalStack.Count <= MaximumStackSize)
            {
                result = new StackCount(Count + additionalStack.Count, MaximumStackSize);
                return true;
            }

            result = default;
            return false;
        }

        public static StackCount OfRaw(ushort count, ushort maxSize)
        {
            return new StackCount(count, maxSize);
        }
        
        public static StackCount Of(int count, int maxSize)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            
            if (count > maxSize)
            {
                throw new ArgumentOutOfRangeException();
            }

            return new StackCount(count, maxSize);
        }

        public bool Equals(StackCount other)
        {
            return CountRaw == other.CountRaw && maximumStackSize == other.maximumStackSize;
        }

        public override bool Equals(object obj)
        {
            return obj is StackCount other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (CountRaw.GetHashCode() * 397) ^ maximumStackSize.GetHashCode();
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