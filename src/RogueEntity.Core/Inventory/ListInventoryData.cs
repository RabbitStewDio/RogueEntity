using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Inventory
{
    /// <summary>
    ///  Technically we should be using fully immutable lists here. 
    /// </summary>
    /// <typeparam name="TOwnerId"></typeparam>
    /// <typeparam name="TItemId"></typeparam>
    [EntityComponent]
    [DataContract]
    [Serializable]
    [MessagePackObject]
    public readonly struct ListInventoryData<TOwnerId, TItemId> : IEquatable<ListInventoryData<TOwnerId, TItemId>>, IContainerView<TItemId>
    {
        [DataMember(Name = nameof(OwnerData), Order = 0)]
        [Key(0)]
        readonly TOwnerId ownerData;

        [DataMember(Name = nameof(TotalWeight), Order = 1)]
        [Key(1)]
        readonly Weight totalWeight;

        [DataMember(Name = nameof(AvailableCarryWeight), Order = 2)]
        [Key(2)]
        readonly Weight availableCarryWeight;

        [DataMember(Name = nameof(Items), Order = 3)]
        [Key(3)]
        readonly List<TItemId> items;

        public ListInventoryData(TOwnerId ownerData, Weight availableCarryWeight) : this()
        {
            this.ownerData = ownerData;
            this.availableCarryWeight = availableCarryWeight;
            this.totalWeight = default;
            this.items = new List<TItemId>();
        }

        [SerializationConstructor]
        public ListInventoryData(TOwnerId ownerData, Weight totalWeight, Weight availableCarryWeight, List<TItemId> items)
        {
            this.ownerData = ownerData;
            this.totalWeight = totalWeight;
            this.availableCarryWeight = availableCarryWeight;
            this.items = new List<TItemId>(items ?? throw new ArgumentNullException(nameof(items)));
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public Weight AvailableCarryWeight
        {
            get { return availableCarryWeight; }
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public Weight TotalWeight
        {
            get { return totalWeight; }
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public TOwnerId OwnerData
        {
            get { return ownerData; }
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public ReadOnlyListWrapper<TItemId> Items
        {
            get { return items; }
        }

        public ListInventoryData<TOwnerId, TItemId> InsertAt(int index, Weight weight, TItemId item)
        {
            var changedItems = new List<TItemId>(this.items);
            changedItems.Insert(index, item);
            var newTotalWeight = TotalWeight + weight;
            var availableWeight = AvailableCarryWeight - weight;
            return new ListInventoryData<TOwnerId, TItemId>(ownerData, newTotalWeight, availableWeight, changedItems);
        }

        public ListInventoryData<TOwnerId, TItemId> Update(int index, Weight weight, TItemId itemId)
        {
            var changedItems = new List<TItemId>(this.items);
            changedItems[index] = itemId;
            var updatedWeight = TotalWeight + weight;
            var availableWeight = AvailableCarryWeight - weight;
            return new ListInventoryData<TOwnerId, TItemId>(ownerData, updatedWeight, availableWeight, changedItems);
        }

        public ListInventoryData<TOwnerId, TItemId> RemoveAt(int itemPosition, Weight weight) 
        {
            var changedItems = new List<TItemId>(this.items);
            changedItems.RemoveAt(itemPosition);

            var updatedWeight = TotalWeight - weight;
            var availableWeight = AvailableCarryWeight + weight;
            return new ListInventoryData<TOwnerId, TItemId>(ownerData, updatedWeight, availableWeight, changedItems);
        }

        public ListInventoryData<TOwnerId, TItemId> RemovePartialStackAt(int itemPosition, Weight weight, TItemId itemId) 
        {
            var changedItems = new List<TItemId>(this.items);
            changedItems[itemPosition] = itemId;
            var updatedWeight = TotalWeight - weight;
            var availableWeight = AvailableCarryWeight + weight;
            return new ListInventoryData<TOwnerId, TItemId>(ownerData, updatedWeight, availableWeight, changedItems);
        }

        public bool Equals(ListInventoryData<TOwnerId, TItemId> other)
        {
            return EqualityComparer<TOwnerId>.Default.Equals(ownerData, other.ownerData) && 
                   totalWeight.Equals(other.totalWeight) && 
                   availableCarryWeight.Equals(other.availableCarryWeight) && 
                   CoreExtensions.EqualsList(items, other.items);
        }

        public override bool Equals(object obj)
        {
            return obj is ListInventoryData<TOwnerId, TItemId> other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EqualityComparer<TOwnerId>.Default.GetHashCode(ownerData);
                hashCode = (hashCode * 397) ^ totalWeight.GetHashCode();
                hashCode = (hashCode * 397) ^ availableCarryWeight.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ListInventoryData<TOwnerId, TItemId> left, ListInventoryData<TOwnerId, TItemId> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ListInventoryData<TOwnerId, TItemId> left, ListInventoryData<TOwnerId, TItemId> right)
        {
            return !left.Equals(right);
        }
    }
}