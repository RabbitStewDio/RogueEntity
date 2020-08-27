using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Inventory
{
    /// <summary>
    ///   Represents the inventory contents of an entity. This inventory is represented as a list of
    ///   items (as opposed to a slotted/grid based inventory). 
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    /// <typeparam name="TOwnerId"></typeparam>
    /// <typeparam name="TItemId"></typeparam>
    public class ListInventory<TGameContext, TOwnerId, TItemId> : IInventory<TGameContext, TItemId>
        where TItemId : IBulkDataStorageKey<TItemId>
        where TOwnerId : IEntityKey
    {
        static readonly ILogger Logger = SLog.ForContext<ListInventory<TGameContext, TOwnerId, TItemId>>();

        readonly IItemResolver<TGameContext, TItemId> itemResolver;
        readonly IItemResolver<TGameContext, TOwnerId> ownerResolver;

        public ListInventory(IItemResolver<TGameContext, TOwnerId> ownerResolver,
                             IItemResolver<TGameContext, TItemId> itemResolver,
                             ListInventoryData<TOwnerId, TItemId> data)
        {
            this.ownerResolver = ownerResolver;
            this.itemResolver = itemResolver;
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
        /// <param name="context"></param>
        /// <param name="r"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public bool TryReAddItemStack(TGameContext context, TItemId r, int slot)
        {
            var itemWeight = itemResolver.QueryWeight(r, context).TotalWeight;
            var insertSlot = Math.Max(0, Math.Min(slot, Data.Items.Count));
            if (r.IsReference)
            {
                if (itemResolver.TryQueryData(r, context, out ContainedInInventoryMarker<TOwnerId, TItemId> owner) &&
                    !owner.IsUnowned())
                {
                    throw new ArgumentException("Do not attempt to use TryReAddItem for general purpose inventory operations");
                }

                if (itemResolver.TryUpdateData(r, context, new ContainedInInventoryMarker<TOwnerId, TItemId>(Data.OwnerData), out var key))
                {
                    Data = Data.InsertAt(insertSlot, itemWeight, r);
                    return true;
                }

                Logger.Warning("ItemReference {ItemReference} could not be marked as owned by this inventory.", r);
                return false;
            }

            var stackSize = itemResolver.QueryStackSize(r, context);
            var itemCount = (int)stackSize.Count;
            Data = Data.InsertAt(insertSlot, itemWeight * itemCount, r);
            return true;
        }

        public bool TryAddItem(TGameContext context,
                               TItemId r,
                               out TItemId remainder,
                               bool ignoreWeight = false)
        {
            if (r.IsReference)
            {
                remainder = default;
                var itemWeight = itemResolver.QueryWeight(r, context).TotalWeight;
                if (!ignoreWeight)
                {
                    if ((TotalWeight + itemWeight) > AvailableCarryWeight)
                    {
                        Logger.Debug("Unable to add Item {ItemReference} as the combined weight of {TotalWeight} and {ItemWeight} exceeds the allowed capacity {AvailableCarryWeight}",
                                     r, TotalWeight, itemWeight, AvailableCarryWeight);
                        return false;
                    }
                }

                if (!TryReleaseFromInventory(context, r, out r))
                {
                    return false;
                }

                if (itemResolver.TryQueryData(r, context, out Position currentPosition) &&
                    !currentPosition.IsInvalid)
                {
                    // This item should not be on a map right now.
                    // This item is misconfigured. 
                    Logger.Warning("ItemReference {ItemReference} is still positioned on the map. Aborting AddItem operation to prevent item duplication.", r);
                    return false;
                }

                if (itemResolver.TryUpdateData(r, context, new ContainedInInventoryMarker<TOwnerId, TItemId>(Data.OwnerData), out _))
                {
                    Data = Data.InsertAt(Data.Items.Count, itemWeight, r);
                    return true;
                }

                Logger.Warning("ItemReference {ItemReference} could not be marked as owned by this inventory.", r);
                return false;
            }

            return AddBulkItem(context, r, out remainder, ignoreWeight);
        }


        bool AddBulkItem(TGameContext context,
                         TItemId r,
                         out TItemId remainder,
                         bool ignoreWeight)
        {
            var itemWeight = itemResolver.QueryWeight(r, context).BaseWeight;
            var stackSize = itemResolver.QueryStackSize(r, context);
            var desiredItemCount = (int)stackSize.Count;
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
                var existingItemStackSize = itemResolver.QueryStackSize(existingItem, context);
                var nextStackSize = existingItemStackSize.Add(itemsLeftToPickUp, out var newItemCount);
                if (nextStackSize == existingItemStackSize)
                {
                    // no change, that stack is already filled.
                    continue;
                }

                if (!itemResolver.TryUpdateData(existingItem, context, nextStackSize, out var changedItem))
                {
                    // unable to modify this item.
                    continue;
                }

                var itemsProcessed = itemsLeftToPickUp - newItemCount;
                Data = Data.Update(index, itemWeight * itemsProcessed, changedItem);
                items[index] = changedItem;
                itemsLeftToPickUp = newItemCount;
                itemsPickedUp += itemsProcessed;
                if (itemsLeftToPickUp <= 0)
                {
                    break;
                }
            }

            while (itemsLeftToPickUp > 0)
            {
                var newStack = StackCount.Of(stackSize.MaximumStackSize).Add(itemsLeftToPickUp, out _);
                if (!itemResolver.TryUpdateData(r, context, newStack, out var changedItem))
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
            if (!itemResolver.TryUpdateData(r, context, stackSize.WithCount((ushort)itemsRemaining), out remainder))
            {
                // Should not happen with all the precautions in the code. Items without stack-size
                // have an implied stack size of 1, and should be caught by the previous two checks.
                Logger.Warning("Unable to update stack count for bulk item. Remainder of item lost.");
                remainder = default;
            }

            return true;
        }

        int ComputeItemsLeftToPickUp(TItemId r, bool ignoreWeight, Weight itemWeight, int desiredItemCount)
        {
            int itemsLeftToPickUp;
            if (!ignoreWeight && itemWeight.WeightInGrams > 0)
            {
                var maxItemsFit = (AvailableCarryWeight - TotalWeight) / itemWeight;
                if (maxItemsFit <= 0)
                {
                    Logger.Debug(
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
        /// <param name="context"></param>
        /// <param name="itemByType"></param>
        /// <param name="itemPosition"></param>
        /// <returns></returns>
        public bool TryRemoveItemStack(TGameContext context, TItemId itemByType, int itemPosition)
        {
            var itemRef = Data.Items[itemPosition];
            if (!itemResolver.IsSameBulkDataType(itemByType, itemRef))
            {
                return false;
            }

            if (itemRef.IsReference)
            {
                if (!itemResolver.TryRemoveData<ContainedInInventoryMarker<TOwnerId, TItemId>>(itemRef, context, out _))
                {
                    Logger.Warning("ItemReference {ItemReference} could not be unmarked as owned by this inventory.", itemRef);
                    return false;
                }
            }

            var stackSize = itemResolver.QueryStackSize(itemRef, context);
            var itemWeight = itemResolver.QueryWeight(itemRef, context);
            Data = Data.RemoveAt(itemPosition, itemWeight.TotalWeight * stackSize.Count);
            return true;
        }

        /// <summary>
        ///   Attempts to remove a number of items that matches the item type. If the item is stackable,
        ///   it will attempt to remove as many items as specified via count from the stack. For reference
        ///    items only the first item is removed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="itemByType"></param>
        /// <param name="removedItem"></param>
        /// <returns></returns>
        public bool TryRemoveItem(TGameContext context, ItemDeclarationId itemByType, out TItemId removedItem)
        {
            for (int i = Data.Items.Count - 1; i >= 0; i--)
            {
                var itemRef = Data.Items[i];
                if (!itemResolver.TryResolve(itemRef, out var itemDef) ||
                    !itemByType.Equals(itemDef.Id))
                {
                    continue;
                }

                if (itemRef.IsReference)
                {
                    if (itemResolver.TryRemoveData<ContainedInInventoryMarker<TOwnerId, TItemId>>(itemRef, context, out _))
                    {
                        var itemWeight = itemResolver.QueryWeight(itemRef, context);
                        Data = Data.RemoveAt(i, itemWeight.TotalWeight);
                        removedItem = itemRef;
                        return true;
                    }

                    Logger.Warning("ItemReference {ItemReference} could not be unmarked as owned by this inventory.", itemRef);
                    removedItem = default;
                    return false;
                }

                if (!itemResolver.SplitStack(context, itemRef, 1, out var takeItem, out var remainingItem, out _))
                {
                    continue;
                }

                var stackSize = itemResolver.QueryStackSize(takeItem, context);
                var takenItemWeight = itemResolver.QueryWeight(takeItem, context);
                var removedWeight = takenItemWeight.TotalWeight * stackSize.Count;
                removedItem = takeItem;

                if (remainingItem.IsEmpty)
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
        /// <param name="context"></param>
        /// <param name="itemByType"></param>
        /// <param name="count"></param>
        /// <param name="removedItems"></param>
        /// <returns></returns>
        public List<TItemId> RemoveBulkItems(TGameContext context,
                                             ItemDeclarationId itemByType,
                                             int count,
                                             List<TItemId> removedItems = null)
        {
            if (removedItems == null)
            {
                removedItems = new List<TItemId>();
            }
            else
            {
                removedItems.Clear();
            }

            for (int i = Data.Items.Count - 1; i >= 0; i--)
            {
                var itemRef = Data.Items[i];
                if (!itemResolver.TryResolve(itemRef, out var itemDef) ||
                    !itemDef.Id.Equals(itemByType))
                {
                    // log warning
                    continue;
                }

                if (itemRef.IsReference)
                {
                    if (itemResolver.TryRemoveData<ContainedInInventoryMarker<TOwnerId, TItemId>>(itemRef, context, out var key))
                    {
                        var itemWeight = itemResolver.QueryWeight(itemRef, context);
                        Data = Data.RemoveAt(i, itemWeight.TotalWeight);
                        removedItems.Add(itemRef);
                        count -= 1;
                    }
                    else
                    {
                        Logger.Warning("ItemReference {ItemReference} could not be unmarked as owned by this inventory.", itemRef);
                    }

                    continue;
                }

                var existingItemStackSize = itemResolver.QueryStackSize(itemRef, context);
                if (existingItemStackSize.Count == 0)
                {
                    // cleanup inventory.
                    Logger.Warning("Inventory contained stale zero-stack item {ItemReference}.", itemRef);
                    Data = Data.RemoveAt(i, default);
                    continue;
                }

                if (existingItemStackSize.Count == 1)
                {
                    var itemWeight = itemResolver.QueryWeight(itemRef, context);
                    Data = Data.RemoveAt(i, itemWeight.TotalWeight);
                    removedItems.Add(itemRef);
                    count -= 1;
                    continue;
                }

                if (!itemResolver.SplitLargeStack(context, itemRef, count, out var takenItem, out var remainingItem, out var stillToBeTaken))
                {
                    // failed to apply split. This only happens if either count is zero (we can exclude that)
                    // or if the (stackable) item cannot update their stack size. At this point the whole
                    // system is probably broken beyond repair.
                    continue;
                }

                var stackCount = itemResolver.QueryStackSize(takenItem, context);
                var takenBaseWeight = itemResolver.QueryWeight(takenItem, context);
                var removedWeight = (takenBaseWeight.TotalWeight * stackCount.Count);

                if (remainingItem.IsEmpty)
                {
                    Data = Data.RemoveAt(i, removedWeight);
                }
                else
                {
                    Data = Data.RemovePartialStackAt(i, removedWeight, remainingItem);
                }

                count = stillToBeTaken;
            }

            return removedItems;
        }

        bool TryReleaseFromInventory(TGameContext context,
                                     TItemId item,
                                     out TItemId removedItem)
        {
            if (item.IsReference)
            {
                if (!itemResolver.TryQueryData(item, context, out ContainedInInventoryMarker<TOwnerId, TItemId> owner))
                {
                    // no one has a claim on this item.
                    removedItem = item;
                    return true;
                }

                if (!TryQueryInventory(context, owner, out var ownerInventory))
                {
                    // is not contained in an actor or item inventory.
                    removedItem = item;
                    return true;
                }

                if (!itemResolver.TryResolve(item, out var itemDef))
                {
                    // should not happen. 
                    Logger.Error("ItemReference {ItemReference} cannot be resolved into an item definition.", item);
                    removedItem = default;
                    return false;
                }

                if (!ownerInventory.TryRemoveItem(context, itemDef.Id, out removedItem))
                {
                    Logger.Warning("ItemReference {ItemReference} was marked as owned by inventory, but owner inventory did not release the item.", item);
                    return false;
                }

                return true;
            }

            // Bulk items are never claimed.
            removedItem = item;
            return true;
        }


        bool TryQueryInventory(TGameContext context,
                               ContainedInInventoryMarker<TOwnerId, TItemId> m,
                               out IInventory<TGameContext, TItemId> inventory)
        {
            if (m.Owner.TryGetValue(out TOwnerId a))
            {
                return ownerResolver.TryQueryData(a, context, out inventory);
            }

            if (m.Container.TryGetValue(out TItemId i))
            {
                return itemResolver.TryQueryData(i, context, out inventory);
            }

            inventory = default;
            return false;
        }
    }
}