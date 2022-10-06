using EnTTSharp;
using System;
using System.Diagnostics.CodeAnalysis;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using Serilog;

namespace RogueEntity.Core.Inventory
{
    /// <summary>
    ///   Represents the inventory contents of an entity. This inventory is represented as a list of
    ///   items (as opposed to a slotted/grid based inventory). 
    /// </summary>
    /// <typeparam name="TOwnerId"></typeparam>
    /// <typeparam name="TItemId"></typeparam>
    public class ListInventory< TOwnerId, TItemId> : IInventory< TItemId>, IEquatable<ListInventory< TOwnerId, TItemId>>
        where TItemId : struct, IEntityKey
        where TOwnerId : struct, IEntityKey
    {
        static readonly ILogger logger = SLog.ForContext<ListInventory< TOwnerId, TItemId>>();

        readonly IItemResolver< TItemId> itemResolver;
        readonly IBulkDataStorageMetaData<TItemId> itemMetaData;

        public ListInventory(IBulkDataStorageMetaData<TItemId> itemMetaData,
                             IItemResolver< TItemId> itemResolver,
                             ListInventoryData<TOwnerId, TItemId> data)
        {
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
            this.itemMetaData = itemMetaData ?? throw new ArgumentNullException(nameof(itemMetaData));
            this.Data = data;
        }

        public ListInventoryData<TOwnerId, TItemId> Data { get; private set; }

        public ReadOnlyListWrapper<TItemId> Items => Data.Items;

        /// <summary>
        ///   Defines the maximum weight the inventory can contain. This is an externally
        ///   defined constant value that scales with either item type (chests etc) or
        ///   character properties (strength in DND rule set).
        /// </summary>
        public Weight AvailableCarryWeight
        {
            get { return Data.AvailableCarryWeight; }
        }

        public Weight TotalWeight
        {
            get { return Data.TotalWeight; }
        }

        /// <summary>
        ///   Undos a TryRemoveItemStack action.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public bool TryReAddItemStack(TItemId r, int slot)
        {
            var itemWeight = itemResolver.QueryWeight(r).TotalWeight;
            var insertSlot = Math.Max(0, Math.Min(slot, Data.Items.Count));
            if (itemMetaData.IsReferenceEntity(r))
            {
                if (itemResolver.TryQueryData<IContainerEntityMarker>(r,  out  _))
                {
                    throw new ArgumentException("Do not attempt to use TryReAddItem for general purpose inventory operations");
                }

                if (itemResolver.TryUpdateData(r,  new ContainerEntityMarker<TOwnerId>(Data.OwnerData), out _))
                {
                    Data = Data.InsertAt(insertSlot, itemWeight, r);
                    return true;
                }

                logger.Warning("ItemReference {ItemReference} could not be marked as owned by this inventory", r);
                return false;
            }

            var stackSize = itemResolver.QueryStackSize(r);
            var itemCount = stackSize.Count;
            Data = Data.InsertAt(insertSlot, itemWeight * itemCount, r);
            return true;
        }

        public bool TryAddItem(TItemId r,
                               out Optional<TItemId> remainder,
                               bool ignoreWeight = false)
        {
            if (itemMetaData.IsReferenceEntity(r))
            {
                var itemWeight = itemResolver.QueryWeight(r).TotalWeight;
                if (!ignoreWeight)
                {
                    if (itemWeight > AvailableCarryWeight)
                    {
                        logger.Debug("Unable to add Item {ItemReference} as the combined weight of {TotalWeight} and {ItemWeight} exceeds the allowed capacity {AvailableCarryWeight}",
                                     r, TotalWeight, itemWeight, AvailableCarryWeight);
                        remainder = default;
                        return false;
                    }
                }

                if (!TryReleaseFromInventory(r))
                {
                    remainder = default;
                    return false;
                }

                if (itemResolver.TryQueryData(r, out Position currentPosition) &&
                    !currentPosition.IsInvalid)
                {
                    // This item should not be on a map right now.
                    // This item is misconfigured. 
                    logger.Warning("ItemReference {ItemReference} is still positioned on the map. Aborting AddItem operation to prevent item duplication", r);
                    remainder = default;
                    return false;
                }

                if (itemResolver.TryUpdateData(r, new ContainerEntityMarker<TOwnerId>(Data.OwnerData), out _))
                {
                    Data = Data.InsertAt(Data.Items.Count, itemWeight, r);
                    remainder = default;
                    return true;
                }

                logger.Warning("ItemReference {ItemReference} could not be marked as owned by this inventory", r);
                remainder = default;
                return false;
            }

            return AddBulkItem(r, out remainder, ignoreWeight);
        }


        bool AddBulkItem(TItemId r,
                         out Optional<TItemId> remainder,
                         bool ignoreWeight)
        {
            var itemWeight = itemResolver.QueryWeight(r).BaseWeight;
            var stackSize = itemResolver.QueryStackSize(r);
            var desiredItemCount = stackSize.Count;
            var itemsLeftToPickUp = ComputeItemsLeftToPickUp(r, ignoreWeight, itemWeight, desiredItemCount);

            if (itemsLeftToPickUp == 0)
            {
                remainder = r;
                return false;
            }

            int itemsPickedUp = 0;
            var items = Data.Items;
            for (var index = 0; index < items.Count; index++)
            {
                var existingItem = items[index];
                var existingItemStackSize = itemResolver.QueryStackSize(existingItem);
                var nextStackSize = existingItemStackSize.Add(itemsLeftToPickUp, out var newItemCount);
                if (nextStackSize == existingItemStackSize)
                {
                    // no change, that stack is already filled.
                    continue;
                }

                if (!itemResolver.TryUpdateData(existingItem, nextStackSize, out var changedItem))
                {
                    // unable to modify this item.
                    continue;
                }

                var itemsProcessed = itemsLeftToPickUp - newItemCount;
                Data = Data.Update(index, itemWeight * itemsProcessed, changedItem);
                itemsLeftToPickUp = newItemCount;
                itemsPickedUp += itemsProcessed;
                if (itemsLeftToPickUp <= 0)
                {
                    break;
                }
            }

            while (itemsLeftToPickUp > 0)
            {
                var newStack = StackCount.Of(Math.Min(itemsLeftToPickUp, stackSize.MaximumStackSize), stackSize.MaximumStackSize);
                if (!itemResolver.TryUpdateData(r, newStack, out var changedItem))
                {
                    // unable to modify this item. Add it as a single instance.
                    Data = Data.InsertAt(Data.Items.Count, itemWeight, r);
                    itemsLeftToPickUp -= 1;
                    itemsPickedUp += 1;
                }
                else
                {
                    var itemsProcessed = newStack.Count;
                    Data = Data.InsertAt(Data.Items.Count, itemWeight * itemsProcessed, changedItem);
                    itemsLeftToPickUp -= itemsProcessed;
                    itemsPickedUp += itemsProcessed;
                }
            }

            var itemsRemaining = desiredItemCount - itemsPickedUp;
            if (itemsRemaining <= 0)
            {
                // everything has been picked up.
                remainder = default;
                return true;
            }

            if (itemsRemaining == desiredItemCount)
            {
                // nothing has been picked up.
                remainder = r;
                return false;
            }

            // Something has been picked up. (This should only apply to stackable items.)
            if (!itemResolver.TryUpdateData(r, stackSize.WithCount((ushort)itemsRemaining), out var remainderRaw))
            {
                // Should not happen with all the precautions in the code. Items without stack-size
                // have an implied stack size of 1, and should be caught by the previous two checks.
                logger.Warning("Unable to update stack count for bulk item. Remainder of item lost");
                remainder = default;
                return false;
            }

            remainder = remainderRaw;
            return true;
        }

        int ComputeItemsLeftToPickUp(TItemId r, bool ignoreWeight, Weight itemWeight, int desiredItemCount)
        {
            int itemsLeftToPickUp;
            if (!ignoreWeight && itemWeight.WeightInGrams > 0)
            {
                var maxItemsFit = AvailableCarryWeight / itemWeight;
                if (maxItemsFit <= 0)
                {
                    logger.Debug(
                        "Unable to add BulkItem {ItemReference} as the combined weight of {TotalWeight} and {ItemWeight} exceeds the allowed capacity {AvailableCarryWeight}",
                        r, TotalWeight, itemWeight * desiredItemCount, AvailableCarryWeight);
                }

                // The maximum of items that can be stored in the inventory.
                itemsLeftToPickUp = Math.Max(0, Math.Min(desiredItemCount, maxItemsFit));
            }
            else
            {
                itemsLeftToPickUp = desiredItemCount;
            }

            return itemsLeftToPickUp;
        }

        /// <summary>
        ///   Attempt to remove the item specified from itemByType from the inventory slot ItemPosition.
        ///   This is a very conservative function to make sure we dont remove items that the player
        ///   hasn't pointed at in case the inventory got reordered.
        /// </summary>
        /// <param name="itemByType"></param>
        /// <param name="itemPosition"></param>
        /// <returns></returns>
        public bool TryRemoveItemStack(TItemId itemByType, int itemPosition)
        {
            if (itemPosition < 0 || itemPosition >= Data.Items.Count)
            {
                return false;
            }

            var itemRef = Data.Items[itemPosition];
            if (!itemMetaData.IsSameBulkType(itemByType, itemRef))
            {
                return false;
            }

            if (itemMetaData.IsReferenceEntity(itemRef))
            {
                if (!itemResolver.TryRemoveData<ContainerEntityMarker<TOwnerId>>(itemRef, out _))
                {
                    logger.Warning("ItemReference {ItemReference} could not be unmarked as owned by this inventory", itemRef);
                    return false;
                }
            }

            var stackSize = itemResolver.QueryStackSize(itemRef);
            var itemWeight = itemResolver.QueryWeight(itemRef);
            Data = Data.RemoveAt(itemPosition, itemWeight.TotalWeight * stackSize.Count);
            return true;
        }

        /// <summary>
        ///   Attempts to remove a number of items that matches the item type. If the item is stackable,
        ///   it will attempt to remove a single item. To remove more than one item of a given type
        ///   use <see cref="TryRemoveItemsInBulk"/>.
        /// </summary>
        /// <param name="itemByType"></param>
        /// <param name="removedItem"></param>
        /// <returns></returns>
        public bool TryRemoveItem( ItemDeclarationId itemByType, out TItemId removedItem)
        {
            for (int i = Data.Items.Count - 1; i >= 0; i--)
            {
                var itemRef = Data.Items[i];
                if (!itemResolver.TryResolve(itemRef, out var itemDef) ||
                    !itemByType.Equals(itemDef.Id))
                {
                    continue;
                }

                if (itemMetaData.IsReferenceEntity(itemRef))
                {
                    if (itemResolver.TryRemoveData<ContainerEntityMarker<TOwnerId>>(itemRef, out _))
                    {
                        var itemWeight = itemResolver.QueryWeight(itemRef);
                        Data = Data.RemoveAt(i, itemWeight.TotalWeight);
                        removedItem = itemRef;
                        return true;
                    }

                    logger.Warning("ItemReference {ItemReference} could not be unmarked as owned by this inventory", itemRef);
                    removedItem = default;
                    return false;
                }

                if (!itemResolver.SplitStack(itemRef, 1, out var takeItemOpt, out var remainingItemOpt, out _) || 
                    takeItemOpt.TryGetValue(out var takeItem))
                {
                    continue;
                }

                var stackSize = itemResolver.QueryStackSize(takeItem);
                var takenItemWeight = itemResolver.QueryWeight(takeItem);
                var removedWeight = takenItemWeight.TotalWeight * stackSize.Count;
                removedItem = takeItem;

                if (!remainingItemOpt.TryGetValue(out var remainingItem))
                {
                    Data = Data.RemoveAt(i, removedWeight);
                }
                else
                {
                    Data = Data.RemovePartialStackAt(i, removedWeight, remainingItem);
                }

                return true;
            }

            removedItem = default;
            return false;
        }

        /// <summary>
        ///   Removes a given number of items in bulk. Works with both reference and stackable
        ///   items.
        /// </summary>
        /// <param name="itemByType"></param>
        /// <param name="count"></param>
        /// <param name="removedItems"></param>
        /// <returns></returns>
        public BufferList<TItemId> TryRemoveItemsInBulk(ItemDeclarationId itemByType,
                                                        int count,
                                                        BufferList<TItemId>? removedItems = null)
        {
            removedItems = BufferList.PrepareBuffer(removedItems);

            for (int i = Data.Items.Count - 1; i >= 0; i--)
            {
                var itemRef = Data.Items[i];
                if (!itemResolver.TryResolve(itemRef, out var itemDef) ||
                    !itemDef.Id.Equals(itemByType))
                {
                    // log warning
                    continue;
                }

                if (itemMetaData.IsReferenceEntity(itemRef))
                {
                    if (itemResolver.TryRemoveData<ContainerEntityMarker<TOwnerId>>(itemRef, out _))
                    {
                        var itemWeight = itemResolver.QueryWeight(itemRef);
                        Data = Data.RemoveAt(i, itemWeight.TotalWeight);
                        removedItems.Add(itemRef);
                        count -= 1;
                    }
                    else
                    {
                        logger.Warning("ItemReference {ItemReference} could not be unmarked as owned by this inventory", itemRef);
                    }

                    continue;
                }

                var existingItemStackSize = itemResolver.QueryStackSize(itemRef);
                if (existingItemStackSize.Count == 0)
                {
                    // cleanup inventory.
                    logger.Warning("Inventory contained stale zero-stack item {ItemReference}", itemRef);
                    Data = Data.RemoveAt(i, default);
                    continue;
                }

                if (existingItemStackSize.Count == 1)
                {
                    var itemWeight = itemResolver.QueryWeight(itemRef);
                    Data = Data.RemoveAt(i, itemWeight.TotalWeight);
                    removedItems.Add(itemRef);
                    count -= 1;
                    continue;
                }

                if (!itemResolver.SplitLargeStack(itemRef, count, out var takenItemOpt, out var remainingItemOpt, out var stillToBeTakenCount) ||
                    !takenItemOpt.TryGetValue(out var takenItem))
                {
                    // failed to apply split. This only happens if either count is zero (we can exclude that)
                    // or if the (stackable) item cannot update their stack size. At this point the whole
                    // system is probably broken beyond repair.
                    continue;
                }

                var takenBaseWeight = itemResolver.QueryWeight(takenItem);
                var removedWeight = (takenBaseWeight.TotalWeight);

                if (!remainingItemOpt.TryGetValue(out var remainingItem))
                {
                    Data = Data.RemoveAt(i, removedWeight);
                }
                else
                {
                    Data = Data.RemovePartialStackAt(i, removedWeight, remainingItem);
                }

                removedItems.Add(takenItem);
                count = stillToBeTakenCount;
            }

            return removedItems;
        }

        bool TryReleaseFromInventory(TItemId item)
        {
            if (itemMetaData.IsReferenceEntity(item))
            {
                if (!itemResolver.TryQueryData<IContainerEntityMarker>(item, out _))
                {
                    // no one has a claim on this item.
                    return true;
                }

                return false;
            }

            // Bulk items are never claimed.
            return true;
        }

        public bool Equals(ListInventory< TOwnerId, TItemId> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Data.Equals(other.Data);
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

            return Equals((ListInventory< TOwnerId, TItemId>)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }

        public static bool operator ==(ListInventory< TOwnerId, TItemId> left, ListInventory< TOwnerId, TItemId> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ListInventory< TOwnerId, TItemId> left, ListInventory< TOwnerId, TItemId> right)
        {
            return !Equals(left, right);
        }
    }
}
