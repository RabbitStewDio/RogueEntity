using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Meta.ItemTraits;
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
}