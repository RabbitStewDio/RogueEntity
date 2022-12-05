using EnTTSharp;
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
        ///   Searches the equipment for an slot that can host the given bulk item. Each item defines a set of
        ///   required and acceptable slots. All required slots are always occupied when the item is equipped.
        ///   Exactly one of the acceptable slots is occupied when the item is equipped.
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
        /// <param name="acceptedSlot">The first slot the item occupies, based on the order of slots listed in the slot requirements</param>
        /// <returns></returns>
        public bool IsBulkEquipmentSpaceAvailable(IItemResolver<TItemId> meta,
                                                  EquipmentSlotRequirements req,
                                                  TItemId bulkItem,
                                                  Optional<EquipmentSlot> desiredSlot,
                                                  [MaybeNullWhen(false)] out EquipmentSlot acceptedSlot)
        {
            if (req.AcceptableSlots.Count == 0 &&
                req.RequiredSlots.Count == 0)
            {
                logger.Verbose("Error: Equipment requirements for item {Item} are empty", bulkItem);
                acceptedSlot = default;
                return false;
            }

            // step 1: Validate the required slots. This may select a primary slot for the item if an compatible item already
            //         occupies the required slots. 
            if (!IsBulkEquipmentRequiredSpaceAvailable(meta, req, bulkItem, out var acceptedSlotTmp))
            {
                // required slots not met.
                acceptedSlot = default;
                return false;
            }
            // if there are required slots, acceptedSlot will be populated.
            // if there are no required slots, accepted slot will be empty.

            // Step 2: An desired slot is given
            if (desiredSlot.TryGetValue(out var desiredSlotValue))
            {
                return IsBulkEquipmentDesiredSpaceAvailable(meta, req, bulkItem, desiredSlotValue, acceptedSlotTmp, out acceptedSlot);
            }

            // if the item has no acceptable slots, it must have had at least one required slot that 
            // we already tested.
            if (req.AcceptableSlots.Count == 0)
            {
                return acceptedSlotTmp.TryGetValue(out acceptedSlot);
            }

            // the item only has acceptable slots, and not required slots. Find the first slot that
            // can accept this item.
            if (req.RequiredSlots.Count == 0)
            {
                // no conflict possible. Select the first available acceptable slot.
                foreach (var equipmentSlot in req.AcceptableSlots)
                {
                    if (!IsSlotAvailableForBulkItem(meta, equipmentSlot, bulkItem, out var primarySlot))
                    {
                        // the slot is already occupied. 
                        continue;
                    }

                    if (primarySlot.TryGetValue(out var ps))
                    {
                        acceptedSlot = ps;
                        return true;
                    }

                    acceptedSlot = equipmentSlot;
                    return true;
                }
            }

            // the item has required slots and acceptable slots.
            // we already processed the required slots, so there should be a valid primary slot already.
            // the fact that the required slot is occupied also means that there exists at least one 
            // acceptable slot that is occupied too.
            return acceptedSlotTmp.TryGetValue(out acceptedSlot);
        }


        bool IsBulkEquipmentDesiredSpaceAvailable(IItemResolver<TItemId> meta,
                                                  EquipmentSlotRequirements req,
                                                  TItemId bulkItem,
                                                  EquipmentSlot desiredSlotValue,
                                                  Optional<EquipmentSlot> acceptedSlotTmp,
                                                  [MaybeNullWhen(false)] out EquipmentSlot acceptedSlot)
        {
            var desiredSlotIsAcceptable = req.AcceptableSlots.Contains(desiredSlotValue);
            var desiredSlotIsRequired = req.RequiredSlots.Contains(desiredSlotValue);
            if (!desiredSlotIsAcceptable && !desiredSlotIsRequired)
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

            EquipmentSlot primarySlot;
            if (!acceptedSlotTmp.TryGetValue(out var prevSelectedPrimarySlot))
            {
                if (usedPrimarySlot.TryGetValue(out primarySlot))
                {
                    // the desired slot is occupied by a compatible item and the item possibly spans multiple slots.
                }
                else
                {
                    // the desired slot is not occupied. Reserve it.
                    primarySlot = desiredSlotValue;
                }
            }
            else if (usedPrimarySlot.TryGetValue(out var usedSlotValue) && prevSelectedPrimarySlot != usedSlotValue)
            {
                logger.Verbose("A desired slot for item {Item} is already occupied by a different item of the same type", bulkItem);
                acceptedSlot = default;
                return false;
            }
            else
            {
                primarySlot = prevSelectedPrimarySlot;
            }

            // Guard against cases where an item has been placed on an alternative acceptable position but
            // occupies the same set of required slots.
            if (desiredSlotIsAcceptable || req.AcceptableSlots.Count == 0)
            {
                acceptedSlot = primarySlot;
                return true;
            }

            // check acceptable slots for conflicts.
            // at least one slot must exist that accepts this item.
            foreach (var a in req.AcceptableSlots)
            {
                if (!IsSlotAvailableForBulkItem(meta, a, bulkItem, out _))
                {
                    // ignored. Not a slot we selected. its incompatible with the item.
                    continue;
                }

                acceptedSlot = primarySlot;
                return true;
            }

            acceptedSlot = default;
            return false;
        }

        /// <summary>
        ///    Checks the required slots of the given item for compatible content. Returns false if any of the
        ///    slots are occupied by an incompatible item. Returns true if the item can occupy all required slots
        ///    or if there are no required slots needed. The primary slot will be filled with the first occupied slot
        ///    if the item requires at least one primary slot. 
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="req"></param>
        /// <param name="bulkItem"></param>
        /// <param name="primarySlot"></param>
        /// <returns></returns>
        bool IsBulkEquipmentRequiredSpaceAvailable(IItemResolver<TItemId> meta,
                                                   EquipmentSlotRequirements req,
                                                   TItemId bulkItem,
                                                   out Optional<EquipmentSlot> primarySlot)
        {
            var acceptedSlot = Optional.Empty<EquipmentSlot>();
            //  Step 1: Check the required slots for the item.
            //  Each required slot must be empty or occupied by a compatible stack.
            foreach (var r in req.RequiredSlots)
            {
                if (!IsSlotAvailableForBulkItem(meta, r, bulkItem, out var usedPrimarySlot))
                {
                    logger.Verbose("A required slot {Slot} for item {Item} is already occupied by an incompatible item", r, bulkItem);
                    primarySlot = default;
                    return false;
                }

                if (!acceptedSlot.TryGetValue(out var prevSelectedSlot))
                {
                    if (usedPrimarySlot.TryGetValue(out var compatiblePrimarySlot))
                    {
                        acceptedSlot = compatiblePrimarySlot;
                    }
                    else
                    {
                        acceptedSlot = r;
                    }
                }
                else if (usedPrimarySlot.TryGetValue(out var slot) && prevSelectedSlot != slot)
                {
                    // if there is an item already occupying the slot, check that it also occupies the same primary slot.
                    // abort if this condition does not hold.
                    logger.Verbose("A required slot {Slot} for item {Item} is already occupied by a different item of the same type", r, bulkItem);
                    primarySlot = default;
                    return false;
                }
            }

            primarySlot = acceptedSlot;
            return true;
        }

        bool IsSlotOccupied(EquipmentSlot equipmentSlot)
        {
            return equippedItems.ContainsKey(equipmentSlot);
        }

        /// <summary>
        ///    Checks if the given bulk item 'r' can be added to the given equipment slot.
        ///    If the slot given is already occupied by an incompatible item, this method returns false.
        ///    If the slot is already occupied by an potentially mergeable item, the method returns true,
        ///    and the item's primary slot will be returned. This handles cases where the item occupies
        ///   multiple slots. We always modify the primary slot.
        ///    
        ///    If the slot is empty, this returns true without providing a primary equipment slot.  
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="equipmentSlot"></param>
        /// <param name="r"></param>
        /// <param name="primarySlot"></param>
        /// <returns></returns>
        bool IsSlotAvailableForBulkItem(IItemResolver<TItemId> meta,
                                        EquipmentSlot equipmentSlot,
                                        TItemId r,
                                        out Optional<EquipmentSlot> primarySlot)
        {
            if (!equippedItems.TryGetValue(equipmentSlot, out var item))
            {
                primarySlot = Optional.Empty();
                return true;
            }

            if (meta.IsSameStackType(item.Reference, r) && item.PrimarySlot != null)
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