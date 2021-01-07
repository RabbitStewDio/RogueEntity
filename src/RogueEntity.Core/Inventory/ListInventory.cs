using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EnTTSharp.Entities;
using JetBrains.Annotations;
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
    /// <typeparam name="TGameContext"></typeparam>
    /// <typeparam name="TOwnerId"></typeparam>
    /// <typeparam name="TItemId"></typeparam>
    public class ListInventory<TGameContext, TOwnerId, TItemId> : IInventory<TGameContext, TItemId>, IEquatable<ListInventory<TGameContext, TOwnerId, TItemId>>
        where TItemId : IEntityKey
        where TOwnerId : IEntityKey
    {
        static readonly ILogger Logger = SLog.ForContext<ListInventory<TGameContext, TOwnerId, TItemId>>();

        readonly IItemResolver<TGameContext, TItemId> itemResolver;
        readonly IBulkDataStorageMetaData<TItemId> itemMetaData;

        public ListInventory([NotNull] IBulkDataStorageMetaData<TItemId> itemMetaData,
                             [NotNull] IItemResolver<TGameContext, TItemId> itemResolver,
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
        /// <param name="context"></param>
        /// <param name="r"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public bool TryReAddItemStack(TGameContext context, TItemId r, int slot)
        {
            var itemWeight = itemResolver.QueryWeight(r, context).TotalWeight;
            var insertSlot = Math.Max(0, Math.Min(slot, Data.Items.Count));
            if (itemMetaData.IsReferenceEntity(r))
            {
                if (itemResolver.TryQueryData(r, context, out IContainerEntityMarker _))
                {
                    throw new ArgumentException("Do not attempt to use TryReAddItem for general purpose inventory operations");
                }

                if (itemResolver.TryUpdateData(r, context, new ContainerEntityMarker<TOwnerId>(Data.OwnerData), out _))
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
            if (itemMetaData.IsReferenceEntity(r))
            {
                remainder = default;
                var itemWeight = itemResolver.QueryWeight(r, context).TotalWeight;
                if (!ignoreWeight)
                {
                    if (itemWeight > AvailableCarryWeight)
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

                if (itemResolver.TryUpdateData(r, context, new ContainerEntityMarker<TOwnerId>(Data.OwnerData), out _))
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
                var maxItemsFit = AvailableCarryWeight / itemWeight;
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
                if (!itemResolver.TryRemoveData<ContainerEntityMarker<TOwnerId>>(itemRef, context, out _))
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
        ///   it will attempt to remove a single item. To remove more than one item of a given type
        ///   use <see cref="TryRemoveItemsInBulk"/>.
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

                if (itemMetaData.IsReferenceEntity(itemRef))
                {
                    if (itemResolver.TryRemoveData<ContainerEntityMarker<TOwnerId>>(itemRef, context, out _))
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
        public BufferList<TItemId> TryRemoveItemsInBulk(TGameContext context,
                                                        ItemDeclarationId itemByType,
                                                        int count,
                                                        BufferList<TItemId> removedItems = null)
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
                    if (itemResolver.TryRemoveData<ContainerEntityMarker<TOwnerId>>(itemRef, context, out _))
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

                var takenBaseWeight = itemResolver.QueryWeight(takenItem, context);
                var removedWeight = (takenBaseWeight.TotalWeight);

                if (remainingItem.IsEmpty)
                {
                    Data = Data.RemoveAt(i, removedWeight);
                }
                else
                {
                    Data = Data.RemovePartialStackAt(i, removedWeight, remainingItem);
                }

                removedItems.Add(takenItem);
                count = stillToBeTaken;
            }

            return removedItems;
        }

        bool TryReleaseFromInventory(TGameContext context,
                                     TItemId item,
                                     out TItemId removedItem)
        {
            if (itemMetaData.IsReferenceEntity(item))
            {
                if (!itemResolver.TryQueryData(item, context, out IContainerEntityMarker _))
                {
                    // no one has a claim on this item.
                    removedItem = item;
                    return true;
                }

                removedItem = default;
                return false;
            }

            // Bulk items are never claimed.
            removedItem = item;
            return true;
        }

        public bool Equals(ListInventory<TGameContext, TOwnerId, TItemId> other)
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

            return Equals((ListInventory<TGameContext, TOwnerId, TItemId>)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }

        public static bool operator ==(ListInventory<TGameContext, TOwnerId, TItemId> left, ListInventory<TGameContext, TOwnerId, TItemId> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ListInventory<TGameContext, TOwnerId, TItemId> left, ListInventory<TGameContext, TOwnerId, TItemId> right)
        {
            return !Equals(left, right);
        }
    }
}
