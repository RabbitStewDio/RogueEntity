using System;
using System.Collections;
using System.Collections.Generic;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Equipment
{
    /// <summary>
    ///   A slotted equipment system that provides an RPG style equipment system.
    ///   Items that can be equipped must contain a EquipmentSlotRequirements data
    ///   trait.
    /// </summary>
    /// <typeparam name="TOwnerId"></typeparam>
    /// <typeparam name="TItemId"></typeparam>
    public class SlottedEquipment<TOwnerId, TItemId> : ISlottedEquipment<TItemId>
        where TItemId : IEntityKey
    {
        static readonly ILogger Logger = SLog.ForContext<SlottedEquipment<TOwnerId, TItemId>>();
        static readonly EqualityComparer<TItemId> ItemEquality = EqualityComparer<TItemId>.Default;
        readonly IItemResolver<TItemId> itemResolver;
        readonly IBulkDataStorageMetaData<TItemId> itemIdMetaData;
        SlottedEquipmentData<TItemId> equippedItems;

        public SlottedEquipment([NotNull] IBulkDataStorageMetaData<TItemId> itemIdMetaData,
                                [NotNull] IItemResolver<TItemId> itemResolver,
                                ReadOnlyListWrapper<EquipmentSlot> availableSlots,
                                SlottedEquipmentData<TItemId> equippedItems,
                                Weight maximumCarryWeight)
        {
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
            this.itemIdMetaData = itemIdMetaData ?? throw new ArgumentNullException(nameof(itemIdMetaData));

            this.equippedItems = equippedItems;
            this.MaximumCarryWeight = maximumCarryWeight;
            AvailableSlots = availableSlots;
        }

        public SlottedEquipment<TOwnerId, TItemId> RefreshWeight()
        {
            var weight = Weight.Empty;
            foreach (var item in equippedItems.Items)
            {
                weight += itemResolver.QueryWeight(item).TotalWeight;
            }

            TotalWeight = weight;
            return this;
        }

        public ReadOnlyListWrapper<EquipmentSlot> AvailableSlots { get; }

        public Weight TotalWeight { get; private set; }

        public Weight MaximumCarryWeight { get; }

        public bool TryQuery(EquipmentSlot slot, out EquippedItem<TItemId> item) => equippedItems.TryGetValue(slot, out item);

        public bool TryUnequipItem(TItemId item, out EquipmentSlot slot)
        {
            if (!itemResolver.TryQueryData(item, out EquipmentSlotRequirements req))
            {
                slot = default;
                return false;
            }

            if (!TryFindItem(item, req, out var itemEquipped))
            {
                slot = default;
                return false;
            }

            slot = itemEquipped.PrimarySlot;
            return TryUnequipItemAt(item, itemEquipped.PrimarySlot);
        }

        bool TryFindItem(TItemId item, EquipmentSlotRequirements req, out EquippedItem<TItemId> result)
        {
            foreach (var slot in req.RequiredSlots)
            {
                if (equippedItems.TryGetValue(slot, out var equippedItem) &&
                    ItemEquality.Equals(equippedItem.Reference, item))
                {
                    result = equippedItem;
                    return true;
                }
            }

            foreach (var slot in req.AcceptableSlots)
            {
                if (equippedItems.TryGetValue(slot, out var equippedItem) &&
                    ItemEquality.Equals(equippedItem.Reference, item))
                {
                    result = equippedItem;
                    return true;
                }
            }

            result = default;
            return false;
        }

        public bool TryUnequipItemAt(TItemId item, EquipmentSlot slot)
        {
            if (!itemResolver.TryQueryData(item, out EquipmentSlotRequirements req))
            {
                return false;
            }

            if (!equippedItems.TryGetValue(slot, out var equippedItem))
            {
                // no item equipped in that slot.
                return false;
            }

            if (!ItemEquality.Equals(equippedItem.Reference, item))
            {
                // the the correct item in that slot
                return false;
            }

            var itemWeight = itemResolver.QueryWeight(item).TotalWeight;
            equippedItems = equippedItems.Remove(equippedItem, req, out var removedItems, out slot);
            TotalWeight -= itemWeight;
            return removedItems;
        }

        public bool TryEquipItem(TItemId item,
                                 out TItemId remainderItem,
                                 Optional<EquipmentSlot> desiredSlot,
                                 out EquipmentSlot actualSlot,
                                 bool ignoreWeightLimits = false)
        {
            return TryEquipItemInternal(item, out remainderItem, desiredSlot, out actualSlot, ignoreWeightLimits);
        }

        bool TryEquipItemInternal(TItemId item,
                                  out TItemId remainderItem,
                                  Optional<EquipmentSlot> desiredSlot,
                                  out EquipmentSlot actualSlot,
                                  bool ignoreWeightLimits)
        {
            if (itemIdMetaData.IsReferenceEntity(item))
            {
                // reference items are always non-stackable, so we can treat it like an atomic unit.

                remainderItem = default;
                if (itemResolver.TryQueryData(item, out IContainerEntityMarker _))
                {
                    Logger.Verbose("Unable to equip reference item {Item} as it is already contained in another container", item);
                    // This item should not be on a map right now.
                    // This item is misconfigured. 
                    actualSlot = default;
                    return false;
                }

                // The item has no map position. That is good, it makes the job easy.
                if (TryEquipItemSingleItem( item, desiredSlot, out actualSlot, ignoreWeightLimits))
                {
                    return true;
                }

                return false;
            }

            var stack = itemResolver.QueryStackSize(item);
            if (stack.Count <= 0)
            {
                Logger.Verbose("Unable to equip item {Item} as it has an empty stack", item);
                remainderItem = default;
                actualSlot = default;
                return false;
            }

            if (stack.MaximumStackSize == 1)
            {
                // Single item, non-stackable, so we can treat it like an atomic unit.
                if (TryEquipItemSingleItem(item, desiredSlot, out actualSlot, ignoreWeightLimits))
                {
                    remainderItem = default;
                    return true;
                }

                actualSlot = default;
                remainderItem = default;
                return false;
            }

            if (!itemResolver.TryQueryData(item, out EquipmentSlotRequirements req))
            {
                Logger.Verbose("Unable to equip item {Item} as it cannot be equipped", item);
                actualSlot = default;
                remainderItem = default;
                return false;
            }

            if (!Data.IsBulkEquipmentSpaceAvailable(itemIdMetaData, req, item, desiredSlot, out actualSlot))
            {
                Logger.Verbose("Unable to equip item {Item} - Not enough space available", item);
                actualSlot = default;
                remainderItem = default;
                return false;
            }

            if (!Data.TryGetValue(actualSlot, out var equippedItem))
            {
                // desired slot is not occupied, so lets just try to fill it as if it is not stacked.
                if (TryEquipItemSingleItem(item, desiredSlot, out actualSlot, ignoreWeightLimits))
                {
                    remainderItem = default;
                    return true;
                }

                actualSlot = default;
                remainderItem = default;
                return false;
            }

            if (!itemIdMetaData.IsSameBulkType(equippedItem.Reference, item))
            {
                // not stackable, the requested slot is occupied by something else.
                actualSlot = default;
                remainderItem = default;
                return false;
            }

            // attempt to  merge the stack.
            var existingStack = itemResolver.QueryStackSize(equippedItem.Reference);
            if (existingStack.Count == existingStack.MaximumStackSize)
            {
                Logger.Verbose("The stacking item already equipped has a full stack; Unable to add more items");
                actualSlot = default;
                remainderItem = default;
                return false;
            }

            var combinedStack = existingStack.Add(stack.Count, out var remainingItems);
            
            if (itemResolver.TryUpdateData(item, combinedStack, out var resultingItem))
            {
                remainderItem = default;
                if (remainingItems > 0 && !itemResolver.TryUpdateData(item, stack.WithCount(remainingItems), out remainderItem))
                {
                    // failed to update remaining item stack. Abort that operation.
                    actualSlot = default;
                    remainderItem = default;
                    return false;
                }
                
                if (TryEquipItemsStackedItem(resultingItem, desiredSlot, out actualSlot, ignoreWeightLimits, equippedItem))
                {
                    return true;
                }
            }


            // find an available slot-set in the current equipment. If there are required slots,
            // all slots must either be empty or all slots must be occupied by the same item.
            // If there are acceptable slots, at least one slot must be empty or occupied matching
            // the result of any required slots.
            actualSlot = default;
            remainderItem = default;
            return false;
        }

        bool TryEquipItemsStackedItem(TItemId item,
                                      Optional<EquipmentSlot> desiredSlot,
                                      out EquipmentSlot actualSlot,
                                      bool ignoreWeight,
                                      EquippedItem<TItemId> currentlyEquippedItem
            )
        {
            if (!itemResolver.TryQueryData(item, out EquipmentSlotRequirements req))
            {
                actualSlot = default;
                return false;
            }

            var existingWeight = itemResolver.QueryWeight(currentlyEquippedItem.Reference).TotalWeight;
            var itemWeight = itemResolver.QueryWeight(item).TotalWeight;
            var totalWeight = TotalWeight - existingWeight;
            if (!ignoreWeight && (totalWeight + itemWeight > MaximumCarryWeight))
            {
                actualSlot = default;
                return false;
            }

            var d = Data.Remove(currentlyEquippedItem, req, out bool successfulRemovedItem, out _);
            if (!successfulRemovedItem)
            {
                actualSlot = default;
                return false;
            }

            if (DoEquip(d, item, req, desiredSlot, out var successEquip, out actualSlot))
            {
                TotalWeight += itemWeight;
                equippedItems = successEquip;
                return true;
            }

            return false;
        }

        bool TryEquipItemSingleItem(TItemId item,
                                    Optional<EquipmentSlot> desiredSlot,
                                    out EquipmentSlot actualSlot,
                                    bool ignoreWeight)
        {
            var itemWeight = itemResolver.QueryWeight(item).TotalWeight;
            if (!ignoreWeight)
            {
                if (TotalWeight + itemWeight > MaximumCarryWeight)
                {
                    Logger.Verbose("Unable to equip item {Item} as this would exceed the weight limits", item);
                    actualSlot = default;
                    return false;
                }
            }

            if (!itemResolver.TryQueryData(item, out EquipmentSlotRequirements req))
            {
                Logger.Verbose("Unable to equip item {Item} as this item cannot be equipped", item);
                actualSlot = default;
                return false;
            }

            if (DoEquip(equippedItems, item, req, desiredSlot, out var successResult, out actualSlot))
            {
                TotalWeight += itemWeight;
                equippedItems = successResult;
                return true;
            }

            return false;
        }

        bool DoEquip(SlottedEquipmentData<TItemId> currentEquippedItems,
                     TItemId item,
                     EquipmentSlotRequirements req,
                     Optional<EquipmentSlot> desiredSlot,
                     out SlottedEquipmentData<TItemId> successResult,
                     out EquipmentSlot actualSlot)
        {
            if (!currentEquippedItems.IsEquipmentSpaceAvailable(req, desiredSlot))
            {
                Logger.Verbose("Unable to equip item {Item} as there is no space available. Desired slot was {DesiredSlot}", item, desiredSlot);
                actualSlot = default;
                successResult = currentEquippedItems;
                return false;
            }

            // All required slots must be filled.
            var slots = new List<EquipmentSlot>();
            if (currentEquippedItems.TryFindAvailableSlot(req, desiredSlot, out var primarySlot, slots) &&
                currentEquippedItems.TryEquip(item, primarySlot, slots, out successResult))
            {
                Logger.Verbose("Successfully stored item {Item} at primary slot {DesiredSlot}", item, desiredSlot);
                actualSlot = primarySlot;
                return true;
            }

            Logger.Verbose("Unable to store item {Item} at primary slot {DesiredSlot}. No available slot", item, desiredSlot);
            successResult = currentEquippedItems;
            actualSlot = default;
            return false;
        }

        public ref SlottedEquipmentData<TItemId> Data
        {
            get { return ref equippedItems; }
        }

        public bool TryReEquipItem(in TItemId targetItem,
                                   EquipmentSlot slot)
        {
            return TryEquipItemSingleItem(targetItem, slot, out _, true);
        }

        public Dictionary<EquipmentSlot, EquippedItem<TItemId>>.ValueCollection.Enumerator GetEnumerator()
        {
            return this.equippedItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<EquippedItem<TItemId>> IEnumerable<EquippedItem<TItemId>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(ISlottedEquipment<TItemId> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (!CoreExtensions.EqualsList(AvailableSlots, other.AvailableSlots))
            {
                return false;
            }

            foreach (var slot in AvailableSlots)
            {
                var my = TryQuery(slot, out var myItem);
                var theirs = other.TryQuery(slot, out var theirItem);
                if (my != theirs)
                {
                    return false;
                }

                if (my && !myItem.Equals(theirItem))
                {
                    return false;
                }
            }

            return true;
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

            return Equals((ISlottedEquipment<TItemId>)obj);
        }

        public override int GetHashCode()
        {
            // there is no sensible hash-code implementation for this class.
            return 0;
        }

        public static bool operator ==(SlottedEquipment<TOwnerId, TItemId> left, SlottedEquipment<TOwnerId, TItemId> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SlottedEquipment<TOwnerId, TItemId> left, SlottedEquipment<TOwnerId, TItemId> right)
        {
            return !Equals(left, right);
        }
    }
}
