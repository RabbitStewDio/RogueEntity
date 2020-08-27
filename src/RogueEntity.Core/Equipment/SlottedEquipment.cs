﻿using System.Collections;
using System.Collections.Generic;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Equipment
{
    public class SlottedEquipment<TGameContext, TItemId> : IEnumerable<EquippedItem<TItemId>>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        static readonly EqualityComparer<TItemId> ItemEquality = EqualityComparer<TItemId>.Default;
        public readonly ReadOnlyListWrapper<EquipmentSlot> AvailableSlots;
        readonly IItemResolver<TGameContext, TItemId> itemResolver;
        SlottedEquipmentData<TItemId> equippedItems;

        public SlottedEquipment(IItemResolver<TGameContext, TItemId> itemResolver,
                                ReadOnlyListWrapper<EquipmentSlot> availableSlots,
                                SlottedEquipmentData<TItemId> equippedItems)
        {
            this.itemResolver = itemResolver;
            this.equippedItems = equippedItems;
            AvailableSlots = availableSlots;
        }

        public bool TryQuery(EquipmentSlot slot, out EquippedItem<TItemId> item) => equippedItems.TryGetValue(slot, out item);


        public bool TryUnequipItem(TGameContext context, TItemId item)
        {
            return TryUnequipItem(context, item, out _);
        }

        public bool TryUnequipItem(TGameContext context, TItemId item, out EquipmentSlot slot)
        {
            if (!itemResolver.TryQueryData(item, context, out EquipmentSlotRequirements req))
            {
                slot = default;
                return false;
            }

            if (!TryFindItem(item, req, out var itemEquipped))
            {
                slot = default;
                return false;
            }

            var itemWeight = itemResolver.QueryWeight(item, context).TotalWeight;
            equippedItems = equippedItems.Remove(itemEquipped, req, itemWeight, out var removedItems, out slot);
            return removedItems;
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

        public bool TryUnequipItemAt(TGameContext context, TItemId item, EquipmentSlot slot)
        {
            if (!itemResolver.TryQueryData(item, context, out EquipmentSlotRequirements req))
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

            var itemWeight = itemResolver.QueryWeight(item, context).TotalWeight;
            equippedItems = equippedItems.Remove(equippedItem, req, itemWeight, out var removedItems, out slot);
            return removedItems;
        }

        public bool TryEquipItem(TGameContext context, TItemId item,
                                 out TItemId modifiedItem,
                                 out EquipmentSlot actualSlot,
                                 ushort count = 0, bool ignoreWeight = false)
        {
            return TryEquipItemInternal(context, item, out modifiedItem, Optional.Empty<EquipmentSlot>(), out actualSlot, count, ignoreWeight);
        }

        public bool TryEquipItem(TGameContext context,
                                 TItemId item,
                                 out TItemId modifiedItem,
                                 Optional<EquipmentSlot> desiredSlot,
                                 out EquipmentSlot actualSlot,
                                 ushort count = 0, bool ignoreWeight = false)
        {
            return TryEquipItemInternal(context, item, out modifiedItem, desiredSlot, out actualSlot, count, ignoreWeight);
        }

        bool TryEquipItemInternal(TGameContext context,
                                  TItemId item,
                                  out TItemId modifiedItem,
                                  Optional<EquipmentSlot> desiredSlot,
                                  out EquipmentSlot actualSlot,
                                  ushort count, bool ignoreWeight)
        {
            if (count == 0)
            {
                count = itemResolver.QueryStackSize(item, context).Count;
            }

            if (item.IsReference)
            {
                if (itemResolver.TryQueryData(item, context, out Position currentPosition) &&
                    !currentPosition.IsInvalid)
                {
                    // This item should not be on a map right now.
                    // This item is misconfigured. 
                    modifiedItem = item;
                    actualSlot = default;
                    return false;
                }

                // The item has no map position. That is good, it makes the job easy.
                if (TryEquipItemInternal(context, item, desiredSlot, out actualSlot, ignoreWeight))
                {
                    modifiedItem = default;
                    return true;
                }

                modifiedItem = item;
                return false;
            }

            if (!itemResolver.SplitStack(context, item, count, out var taken, out var remainder, out var remaining))
            {
                modifiedItem = item;
                actualSlot = default;
                return false;
            }

            if (remaining != 0)
            {
                modifiedItem = item;
                actualSlot = default;
                return false;
            }

            if (TryEquipItemInternal(context, taken, desiredSlot, out actualSlot, ignoreWeight))
            {
                modifiedItem = remainder;
                return true;
            }

            modifiedItem = item;
            return false;
        }

        bool TryEquipItemInternal(TGameContext context, TItemId item,
                                  Optional<EquipmentSlot> desiredSlot,
                                  out EquipmentSlot actualSlot,
                                  bool ignoreWeight = false)
        {
            var itemCount = itemResolver.QueryStackSize(item, context).Count;
            var itemWeight = itemResolver.QueryWeight(item, context).BaseWeight;
            var weight = itemWeight * itemCount;
            if (!ignoreWeight)
            {
                if ((equippedItems.TotalWeight + weight) > equippedItems.AvailableCarryWeight)
                {
                    actualSlot = default;
                    return false;
                }
            }

            if (!itemResolver.TryQueryData(item, context, out EquipmentSlotRequirements req))
            {
                actualSlot = default;
                return false;
            }

            equippedItems = equippedItems.Equip(item, req, desiredSlot, weight, out var success, out actualSlot);
            return success;
        }

        public ref SlottedEquipmentData<TItemId> Data
        {
            get { return ref equippedItems; }
        }

        public bool TryReEquipItem(TGameContext context,
                                   in TItemId targetItem,
                                   EquipmentSlot slot)
        {
            return TryEquipItemInternal(context, targetItem, slot, out _, true);
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
    }
}