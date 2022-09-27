using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Utils;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Equipment
{
    [EntityComponent]
    [Serializable]
    [DataContract]
    [MessagePackObject]
    public readonly struct SlottedEquipmentData<TItemId> : IEquatable<SlottedEquipmentData<TItemId>>, IContainerView<TItemId>
        where TItemId : struct, IEntityKey
    {
        static readonly ILogger logger = SLog.ForContext<SlottedEquipmentData<TItemId>>();

        [DataMember(Name = "EquippedItems", Order = 0)]
        [Key(0)]
        readonly Dictionary<EquipmentSlot, EquippedItem<TItemId>> equippedItems;

        [IgnoreMember]
        [IgnoreDataMember]
        public ReadOnlyListWrapper<TItemId> Items => RebuildEquipmentList();

        [SerializationConstructor]
        internal SlottedEquipmentData(Dictionary<EquipmentSlot, EquippedItem<TItemId>> equippedItems)
        {
            this.equippedItems = equippedItems ?? throw new ArgumentNullException(nameof(equippedItems));
        }

        List<TItemId> RebuildEquipmentList()
        {
            var equippedItemList = new List<TItemId>();
            foreach (var r in equippedItems)
            {
                if (r.Key == r.Value.PrimarySlot && 
                    !r.Value.Reference.IsEmpty)
                {
                    equippedItemList.Add(r.Value.Reference);
                }
            }

            return equippedItemList;
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext sc)
        {
            RebuildEquipmentList();
        }

        public bool TryGetValue(EquipmentSlot key, out EquippedItem<TItemId> value)
        {
            return equippedItems.TryGetValue(key, out value);
        }

        public static SlottedEquipmentData<TItemId> Create()
        {
            return new SlottedEquipmentData<TItemId>(new Dictionary<EquipmentSlot, EquippedItem<TItemId>>());
        }

        public bool TryEquip(TItemId item, EquipmentSlot primarySlot, List<EquipmentSlot> occupiedSlots, out SlottedEquipmentData<TItemId> result)
        {
            var foundPrimary = false;
            foreach (var s in occupiedSlots)
            {
                if (s == primarySlot)
                {
                    foundPrimary = true;
                }

                if (IsSlotOccupied(s))
                {
                    result = this;
                    return false;
                }
            }

            if (!foundPrimary)
            {
                result = this;
                return false;
            }

            var data = new EquippedItem<TItemId>(item, primarySlot);
            var d = new Dictionary<EquipmentSlot, EquippedItem<TItemId>>(equippedItems);
            foreach (var s in occupiedSlots)
            {
                d[s] = data;
            }

            result = new SlottedEquipmentData<TItemId>(d);
            return true;
        }

        public SlottedEquipmentData<TItemId> Remove(EquippedItem<TItemId> itemToRemove,
                                                    EquipmentSlotRequirements req,
                                                    out bool removedItem,
                                                    out EquipmentSlot? slot)
        {
            removedItem = false;
            slot = default;

            var d = new Dictionary<EquipmentSlot, EquippedItem<TItemId>>(equippedItems);
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
                return new SlottedEquipmentData<TItemId>(d);
            }

            return this;
        }

        public bool TryFindAvailableSlot(EquipmentSlotRequirements req,
                                         Optional<EquipmentSlot> desiredSlot,
                                         [MaybeNullWhen(false)] out EquipmentSlot primarySlot,
                                         List<EquipmentSlot> occupiedSlots)
        {
            var success = false;
            primarySlot = default;
            foreach (var r in req.RequiredSlots)
            {
                if (!success)
                {
                    success = true;
                    primarySlot = r;
                }

                occupiedSlots.Add(r);
            }

            if (req.AcceptableSlots.Count > 0)
            {
                if (desiredSlot.TryGetValue(out var desiredSlotValue))
                {
                    if (!success)
                    {
                        success = true;
                        primarySlot = desiredSlotValue;
                    }

                    occupiedSlots.Add(desiredSlotValue);
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
                            primarySlot = r;
                            success = true;
                        }

                        occupiedSlots.Add(r);
                        break;
                    }
                }
            }

            return success;
        }

        public bool IsEquipmentSpaceAvailable(EquipmentSlotRequirements req,
                                              Optional<EquipmentSlot> desiredSlot)
        {
            if (req.AcceptableSlots.Count == 0 &&
                req.RequiredSlots.Count == 0)
            {
                logger.Verbose("Equipment requirements for item are empty");
                return false;
            }

            if (desiredSlot.TryGetValue(out var desiredSlotValue))
            {
                if (!req.AcceptableSlots.Contains(desiredSlotValue) &&
                    !req.RequiredSlots.Contains(desiredSlotValue))
                {
                    logger.Verbose("Desired slot {DesiredSlotValue} is not valid for the given item", desiredSlotValue);
                    return false;
                }

                if (IsSlotOccupied(desiredSlotValue))
                {
                    logger.Verbose("Desired slot {DesiredSlotValue} is already occupied", desiredSlotValue);
                    return false;
                }
            }

            foreach (var r in req.RequiredSlots)
            {
                if (IsSlotOccupied(r))
                {
                    logger.Verbose("Required slot {Slot} already occupied", desiredSlotValue);
                    return false;
                }
            }

            if (req.AcceptableSlots.Count == 0)
            {
                return true;
            }

            foreach (var r in req.AcceptableSlots)
            {
                if (!IsSlotOccupied(r))
                {
                    return true;
                }
            }

            logger.Verbose("All acceptable slots {Slot} are already occupied", desiredSlotValue);
            return false;
        }

        /// <summary>
        ///   Searches the equipment for an slot that can host the given bulk item.
        /// </summary>
        /// <remarks>
        /// <para>
        ///  A suitable equipment slot must satisfy the following constraints
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// all slots must be either empty or occupied by an bulk item of the same type.
        /// all used slots must reference the same bulk item (as evidenced by pointing to the same primary slot)  
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="meta"></param>
        /// <param name="req"></param>
        /// <param name="bulkItem"></param>
        /// <param name="desiredSlot"></param>
        /// <param name="acceptedSlot"></param>
        /// <returns></returns>
        public bool IsBulkEquipmentSpaceAvailable(IBulkDataStorageMetaData<TItemId> meta,
                                                  EquipmentSlotRequirements req,
                                                  TItemId bulkItem,
                                                  Optional<EquipmentSlot> desiredSlot,
                                                  [MaybeNullWhen(false)] out EquipmentSlot acceptedSlot)
        {
            if (req.AcceptableSlots.Count == 0 &&
                req.RequiredSlots.Count == 0)
            {
                logger.Verbose("Equipment requirements for item {Item} are empty", bulkItem);
                acceptedSlot = default;
                return false;
            }

            var slotRetrieved = false;
            EquipmentSlot? acceptedSlotTmp = null;
            foreach (var r in req.RequiredSlots)
            {
                if (!IsSlotAvailableForBulkItem(meta, r, bulkItem, out var usedSlot))
                {
                    logger.Verbose("A required slot {Slot} for item {Item} is already occupied by an incompatible item", r, bulkItem);
                    acceptedSlot = default;
                    return false;
                }

                if (!slotRetrieved)
                {
                    slotRetrieved = true;
                    acceptedSlotTmp = r;
                }
                else if (usedSlot.TryGetValue(out var slot) && acceptedSlotTmp != slot)
                {
                    logger.Verbose("A required slot {Slot} for item {Item} is already occupied by a different item of the same type", r, bulkItem);
                    acceptedSlot = default;
                    return false;
                }
            }

            if (desiredSlot.TryGetValue(out var desiredSlotValue))
            {
                if (!req.AcceptableSlots.Contains(desiredSlotValue) &&
                    !req.RequiredSlots.Contains(desiredSlotValue))
                {
                    // filter out garbage inputs.
                    logger.Verbose("Given desired slot does not match the available slots defined for bulk item {BulkItem}", bulkItem);
                    acceptedSlot = default;
                    return false;
                }

                if (!IsSlotAvailableForBulkItem(meta, desiredSlotValue, bulkItem, out var usedPrimarySlot))
                {
                    logger.Verbose("A desired slot for item {Item} is already occupied by an incompatible item", bulkItem);
                    acceptedSlot = default;
                    return false;
                }

                if (!slotRetrieved)
                {
                    acceptedSlotTmp = desiredSlotValue;
                }
                else if (usedPrimarySlot.TryGetValue(out var usedSlotValue) && acceptedSlotTmp != usedSlotValue)
                {
                    logger.Verbose("A desired slot for item {Item} is already occupied by a different item of the same type", bulkItem);
                    acceptedSlot = default;
                    return false;
                }

                // Guard against cases where an item has been placed on an alternative acceptable position but
                // occupies the same set of primary slots.
                foreach (var a in req.AcceptableSlots)
                {
                    if (a == desiredSlotValue)
                    {
                        // ignored. We know this slot is ok to select.
                        continue;
                    }

                    if (!IsSlotAvailableForBulkItem(meta, a, bulkItem, out var usedSlot))
                    {
                        // ignored. Not a slot we selected. its incompatible with the item.
                        continue;
                    }

                    if (!usedSlot.TryGetValue(out var slotVal))
                    {
                        // ignored. The slot is empty, not not selected via the desired slot value.
                        continue;
                    }

                    if (slotVal == acceptedSlotTmp)
                    {
                        logger.Verbose("A conflicting bulk item has been found and occupies the same primary slot");
                        acceptedSlot = default;
                        return false;
                    }
                }

                if (acceptedSlotTmp == null)
                {
                    acceptedSlot = default;
                    return false;
                }
                
                acceptedSlot = acceptedSlotTmp;
                return true;
            }

            if (req.AcceptableSlots.Count == 0)
            {
                acceptedSlot = acceptedSlotTmp;
                return slotRetrieved;
            }

            foreach (var r in req.AcceptableSlots)
            {
                if (!IsSlotAvailableForBulkItem(meta, r, bulkItem, out var usedSlot))
                {
                    continue;
                }

                if (!slotRetrieved)
                {
                    acceptedSlot = r;
                    return true;
                }

                if (!usedSlot.TryGetValue(out var usedSlotValue) || acceptedSlotTmp == usedSlotValue)
                {
                    acceptedSlot = usedSlotValue;
                    return true;
                }
            }

            logger.Verbose("A unoccupied acceptable slot for item {Item} was not found", bulkItem);
            acceptedSlot = default;
            return false;
        }

        bool IsSlotOccupied(EquipmentSlot equipmentSlot)
        {
            return equippedItems.ContainsKey(equipmentSlot);
        }

        bool IsSlotAvailableForBulkItem(IBulkDataStorageMetaData<TItemId> meta,
                                        EquipmentSlot equipmentSlot,
                                        TItemId r,
                                        out Optional<EquipmentSlot> primarySlot)
        {
            if (!equippedItems.TryGetValue(equipmentSlot, out var item))
            {
                primarySlot = Optional.Empty();
                return true;
            }

            if (meta.IsSameBulkType(item.Reference, r) && item.PrimarySlot != null)
            {
                primarySlot = item.PrimarySlot;
                return true;
            }

            primarySlot = default;
            return false;
        }

        public Dictionary<EquipmentSlot, EquippedItem<TItemId>>.ValueCollection.Enumerator GetEnumerator() => equippedItems.Values.GetEnumerator();

        public bool Equals(SlottedEquipmentData<TItemId> other)
        {
            return CoreExtensions.EqualsDictionary(equippedItems, other.equippedItems);
        }

        public override bool Equals(object obj)
        {
            return obj is SlottedEquipmentData<TItemId> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return equippedItems.Count;
        }

        public static bool operator ==(SlottedEquipmentData<TItemId> left, SlottedEquipmentData<TItemId> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SlottedEquipmentData<TItemId> left, SlottedEquipmentData<TItemId> right)
        {
            return !left.Equals(right);
        }
    }
}