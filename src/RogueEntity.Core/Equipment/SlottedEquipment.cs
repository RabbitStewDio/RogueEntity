using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EnTTSharp.Annotations;
using MessagePack;
using RogueEntity.Core.Infrastructure.Meta;
using RogueEntity.Core.Infrastructure.Meta.Items;
using RogueEntity.Core.Infrastructure.Positioning.Grid;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Equipment
{
    [EntityComponent]
    [Serializable]
    [DataContract]
    [MessagePackObject]
    public readonly struct SlottedEquipmentData<TItemId>
    {
        [DataMember(Name = nameof(Count), Order = 0)]
        [Key(0)]
        readonly int count;
        [DataMember(Name = nameof(TotalWeight), Order = 1)]
        [Key(1)]
        readonly Weight totalWeight;
        [DataMember(Name = nameof(AvailableCarryWeight), Order = 2)]
        [Key(2)]
        readonly Weight availableCarryWeight;
        [DataMember(Name = "EquippedItems", Order = 3)]
        [Key(3)]
        readonly Dictionary<EquipmentSlot, EquippedItem<TItemId>> equippedItems;

        [SerializationConstructor]
        internal SlottedEquipmentData(int count,
                             Weight totalWeight,
                             Weight availableCarryWeight,
                             Dictionary<EquipmentSlot, EquippedItem<TItemId>> equippedItems)
        {
            this.equippedItems = equippedItems;
            this.count = count;
            this.totalWeight = totalWeight;
            this.availableCarryWeight = availableCarryWeight;
        }

        public int Count
        {
            get { return count; }
        }

        public Weight TotalWeight
        {
            get { return totalWeight; }
        }

        public Weight AvailableCarryWeight
        {
            get { return availableCarryWeight; }
        }

        public bool TryGetValue(EquipmentSlot key, out EquippedItem<TItemId> value)
        {
            return equippedItems.TryGetValue(key, out value);
        }

        public static SlottedEquipmentData<TItemId> Create()
        {
            return new SlottedEquipmentData<TItemId>(0, default, default, new Dictionary<EquipmentSlot, EquippedItem<TItemId>>());
        }

        public SlottedEquipmentData<TItemId> Remove(EquippedItem<TItemId> itemToRemove, EquipmentSlotRequirements req,
                                                    Weight itemWeight,
                                                    out bool removedItem,
                                                    out EquipmentSlot slot)
        {
            var d = new Dictionary<EquipmentSlot, EquippedItem<TItemId>>(equippedItems);
            removedItem = false;
            slot = default;

            foreach (var r in req.RequiredSlots)
            {
                if (d.TryGetValue(r, out var containedItem) &&
                    containedItem == itemToRemove)
                {
                    slot = containedItem.PrimarySlot;
                    d.Remove(r);
                    removedItem = true;
                }
            }

            foreach (var r in req.AcceptableSlots)
            {
                if (d.TryGetValue(r, out var containedItem) &&
                    containedItem == itemToRemove)
                {
                    slot = containedItem.PrimarySlot;
                    d.Remove(r);
                    removedItem = true;
                    break;
                }
            }

            if (removedItem)
            {
                var newTotalWeight = TotalWeight - itemWeight;
                var newAvailableCarryWeight = AvailableCarryWeight + itemWeight;
                var newCount = Count - 1;
                return new SlottedEquipmentData<TItemId>(newCount, newTotalWeight, newAvailableCarryWeight, d);
            }

            return this;
        }

        public SlottedEquipmentData<TItemId> Equip(TItemId item,
                                                   EquipmentSlotRequirements req,
                                                   Optional<EquipmentSlot> desiredSlot,
                                                   Weight itemWeight,
                                                   out bool success,
                                                   out EquipmentSlot actualSlot)
        {
            if ((req.RequiredSlots.Count == 0 && req.AcceptableSlots.Count == 0) ||
                !IsEquipmentSpaceAvailable(req))
            {
                actualSlot = default;
                success = false;
                return this;
            }


            if (req.AcceptableSlots.Count > 0)
            {
                if (desiredSlot.TryGetValue(out var desiredSlotValue))
                {
                    if (!req.AcceptableSlots.Contains(desiredSlotValue))
                    {
                        actualSlot = default;
                        success = false;
                        return this;
                    }

                    if (IsSlotOccupied(desiredSlotValue))
                    {
                        actualSlot = default;
                        success = false;
                        return this;
                    }
                }
            }


            // All required slots must be filled.

            var d = new Dictionary<EquipmentSlot, EquippedItem<TItemId>>(equippedItems);
            success = false;
            EquippedItem<TItemId> data = default;
            foreach (var r in req.RequiredSlots)
            {
                if (!success)
                {
                    success = true;
                    data = new EquippedItem<TItemId>(item, r);
                    d[r] = data;
                }
                d[r] = data;
            }

            if (req.AcceptableSlots.Count > 0)
            {
                if (desiredSlot.TryGetValue(out var desiredSlotValue))
                {
                    if (!success)
                    {
                        data = new EquippedItem<TItemId>(item, desiredSlotValue);
                        success = true;
                    }

                    d[desiredSlotValue] = data;
                }
                else
                {
                    foreach (var r in req.AcceptableSlots)
                    {
                        if (IsSlotOccupied(r))
                        {
                            continue;
                        }

                        if (!success)
                        {
                            data = new EquippedItem<TItemId>(item, r);
                            success = true;
                        }
                        d[r] = data;
                        break;
                    }
                }
            }

            if (!success)
            {
                actualSlot = default;
                return this;
            }

            var newCount = Count + 1;
            var newTotalWeight = TotalWeight + itemWeight;
            var newAvailableWeight = AvailableCarryWeight - itemWeight;
            actualSlot = data.PrimarySlot;
            return new SlottedEquipmentData<TItemId>(newCount, newTotalWeight, newAvailableWeight, d);
        }


        bool IsEquipmentSpaceAvailable(EquipmentSlotRequirements req)
        {
            if (req.AcceptableSlots.Count == 0 &&
                req.RequiredSlots.Count == 0)
            {
                return false;
            }

            foreach (var r in req.RequiredSlots)
            {
                if (IsSlotOccupied(r))
                {
                    return false;
                }
            }

            foreach (var r in req.AcceptableSlots)
            {
                if (!IsSlotOccupied(r))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsSlotOccupied(EquipmentSlot equipmentSlot)
        {
            return equippedItems.ContainsKey(equipmentSlot);
        }

        public Dictionary<EquipmentSlot, EquippedItem<TItemId>>.ValueCollection.Enumerator GetEnumerator() => equippedItems.Values.GetEnumerator();
    }

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
                if (itemResolver.TryQueryData(item, context, out EntityGridPosition existingMapPosition))
                {
                    if (!itemResolver.TryRemoveData<EntityGridPosition>(item, context, out item))
                    {
                        // Unable to equip this item if we cannot remove it from the map.
                        modifiedItem = item;
                        actualSlot = default;
                        return false;
                    }

                    if (TryEquipItemInternal(context, item, desiredSlot, out actualSlot, ignoreWeight))
                    {
                        modifiedItem = default;
                        return true;
                    }

                    if (!itemResolver.TryUpdateData(item, context, existingMapPosition, out item))
                    {
                        // Unable to undo the removal of the item from the map. This is bad.
                        // But given that the first update succeeded, this should really not happen.
                        throw new InvalidOperationException("Unable to undo failed equip attempt.");
                    }

                    modifiedItem = item;
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