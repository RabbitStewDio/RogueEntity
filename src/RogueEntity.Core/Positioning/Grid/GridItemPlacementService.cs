using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.Positioning.Grid
{
    public class GridItemPlacementService<TItemId> : IItemPlacementService<TItemId>
        where TItemId : struct, IEntityKey
    {
        public event EventHandler<ItemPositionChangedEvent<TItemId>>? ItemPositionChanged;
        static readonly EqualityComparer<TItemId> equality = EqualityComparer<TItemId>.Default;
        static readonly ILogger logger = SLog.ForContext<GridItemPlacementService<TItemId>>();

        readonly IBulkDataStorageMetaData<TItemId> itemIdMetaData;
        readonly IItemResolver<TItemId> itemResolver;
        readonly IGridMapContext<TItemId> mapContext;

        public GridItemPlacementService(IItemResolver<TItemId> itemResolver,
                                        IGridMapContext<TItemId> mapContext)
        {
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
            this.mapContext = mapContext ?? throw new ArgumentNullException(nameof(mapContext));
            this.itemIdMetaData = itemResolver.EntityMetaData;
        }

        public bool TryQueryItem<TPosition>(in TPosition placementPos, out TItemId item)
            where TPosition : IPosition<TPosition>
        {
            if (placementPos.IsInvalid)
            {
                logger.Verbose("Given position {Position} is invalid", placementPos);
                item = default;
                return false;
            }

            if (placementPos.LayerId == MapLayer.Indeterminate.LayerId)
            {
                item = default;
                return false;
            }

            if (!mapContext.TryGetGridDataFor(placementPos.LayerId, out var mapData))
            {
                logger.Verbose("Unable to resolve grid data for map layer {LayerId} of position {Position}", placementPos.LayerId, placementPos);
                item = default;
                return false;
            }

            if (!mapData.TryGetView(placementPos.GridZ, out var map))
            {
                logger.Verbose("Requested grid position for map layer {LayerId} is out of range for position {Position}", placementPos.LayerId, placementPos);
                item = default;
                return false;
            }

            return map.TryGet(placementPos.GridX, placementPos.GridY, out item);
        }

        /// <summary>
        ///   Tries to remove the target item from the map. The item will not be destroyed
        ///   in the process. Use this to place items into containers or generally leave
        ///   them outside of the physical realm.
        /// </summary>
        /// <param name="targetItem"></param>
        /// <param name="placementPos"></param>
        /// <returns></returns>
        public bool TryRemoveItem<TPosition>(in TItemId targetItem, in TPosition placementPos)
            where TPosition : IPosition<TPosition>
        {
            if (targetItem.IsEmpty)
            {
                return true;
            }

            if (placementPos.IsInvalid)
            {
                logger.Verbose("Given position {Position} is invalid", placementPos);
                return false;
            }

            if (placementPos.LayerId == MapLayer.Indeterminate.LayerId)
            {
                return false;
            }

            if (!mapContext.TryGetGridDataFor(placementPos.LayerId, out var mapData))
            {
                logger.Verbose("Unable to resolve grid data for map layer {LayerId} of position {Position}", placementPos.LayerId, placementPos);
                return false;
            }

            if (!mapData.TryGetWritableView(placementPos.GridZ, out var map))
            {
                logger.Verbose("Requested grid position for map layer {LayerId} is out of range for position {Position}", placementPos.LayerId, placementPos);
                return false;
            }

            var defaultItem = default(TItemId);
            ref var existingItem = ref map.TryGetForUpdate(placementPos.GridX, placementPos.GridY, ref defaultItem, out var success);
            if (!success)
            {
                logger.Verbose("Unable to query item at {Position}", placementPos);
                return false;
            }

            Assert.NotNull(existingItem);
            
            // Check the easy path: Is this a reference item?
            if (itemIdMetaData.IsReferenceEntity(targetItem))
            {
                if (!equality.Equals(existingItem, targetItem))
                {
                    logger.Verbose("Safety check failed: Unable to locate source item {ExistingItem} at position {Position}", targetItem, placementPos);
                    return false;
                }

                if (!itemResolver.TryUpdateData(targetItem, EntityGridPositionUpdateMessage.From(EntityGridPosition.Invalid), out _))
                {
                    logger.Warning("Unable to update entity data for position update on entity {ExistingItem}", targetItem);
                    return false;
                }

                existingItem = default;
                mapData.MarkDirty(placementPos);
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForRemove(targetItem, placementPos));
                return true;
            }

            // Not so easy: Bulk items require special handling as they can be stackable.
            if (!itemIdMetaData.IsSameBulkType(targetItem, existingItem))
            {
                logger.Verbose("Safety check failed: Unable to locate the correct bulk item type for {ExistingItem} at position {Position}", targetItem, placementPos);
                return false;
            }

            var stackSize = itemResolver.QueryStackSize(targetItem);
            if (stackSize.MaximumStackSize == 1)
            {
                // The slightly easy path: This is a bulk item that is not stackable
                existingItem = default;
                mapData.MarkDirty(placementPos);
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForRemove(targetItem, placementPos));
                return true;
            }

            var existingStack = itemResolver.QueryStackSize(existingItem);
            var takeResult = existingStack.Take(stackSize.Count);
            if (takeResult.ItemsNotAvailableInStack != 0)
            {
                // the stack at the map position does not contain enough items 
                // to satisfy the requested stack size of targetItem' 
                return false;
            }

            // Stackable bulk item. We reduced the stack by the number of items given.

            if (takeResult.ItemsLeftInStack.TryGetValue(out var resultStack))
            {
                if (!itemResolver.TryUpdateData(targetItem, resultStack, out var changedItem))
                {
                    // for some unlikely reason we are not allowed to write the resulting stack back 
                    // to the item. 
                    //
                    // Either way we are done here, we cannot proceed.
                    return false;
                }

                existingItem = changedItem;
                mapData.MarkDirty(placementPos);
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForRemove(targetItem, placementPos));
                return true;
            }

            // The resulting item stack is empty, so we can fully discard the item from the map.
            existingItem = default;
            mapData.MarkDirty(placementPos);
            ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForRemove(targetItem, placementPos));
            return true;
        }

        /// <summary>
        ///    Places an item at the given placement position. It is assumed and strongly
        ///    recommended that the item has not been placed elsewhere (and is validated for
        ///    reference items).
        /// </summary>
        /// <param name="targetItem"></param>
        /// <param name="placementPos"></param>
        /// <returns></returns>
        public bool TryPlaceItem<TPosition>(in TItemId targetItem, in TPosition placementPos)
            where TPosition : IPosition<TPosition>
        {
            if (placementPos.IsInvalid)
            {
                logger.Verbose("Given position {Position} is invalid", placementPos);
                return false;
            }

            if (placementPos.LayerId == MapLayer.Indeterminate.LayerId)
            {
                return false;
            }

            if (!mapContext.TryGetGridDataFor(placementPos.LayerId, out var mapData))
            {
                logger.Verbose("Unable to resolve grid data for map layer {LayerId} of position {Position}", placementPos.LayerId, placementPos);
                return false;
            }

            if (!mapData.TryGetWritableView(placementPos.GridZ, out var map, DataViewCreateMode.CreateMissing))
            {
                logger.Verbose("Requested grid position for map layer {LayerId} is out of range for position {Position}", placementPos.LayerId, placementPos);
                return false;
            }

            var defaultItem = default(TItemId);
            ref var existingItem = ref map.TryGetForUpdate(placementPos.GridX, placementPos.GridY, ref defaultItem, out var success, DataViewCreateMode.CreateMissing);
            if (!success)
            {
                logger.Verbose("Unable to query item at {Position}", placementPos);
                return false;
            }
            Assert.NotNull(existingItem);

            if (itemIdMetaData.IsReferenceEntity(targetItem))
            {
                if (!existingItem.IsEmpty)
                {
                    logger.Verbose("Precondition check failed: Placement position is not empty at {Position}", placementPos);
                    return false;
                }

                if (!itemResolver.TryUpdateData(targetItem, new EntityGridPositionUpdateMessage(EntityGridPosition.From(placementPos)), out _))
                {
                    logger.Verbose("Failed to update entity data for {TargetItem} at position {Position}", targetItem, placementPos);
                    return false;
                }

                existingItem = targetItem;
                mapData.MarkDirty(placementPos);
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForPlacement(targetItem, placementPos));
                return true;
            }

            if (existingItem.IsEmpty)
            {
                existingItem = targetItem;
                mapData.MarkDirty(placementPos);
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForPlacement(targetItem, placementPos));
                return true;
            }

            if (!itemIdMetaData.IsSameBulkType(targetItem, existingItem))
            {
                logger.Verbose("Precondition check failed: Placement position is not empty or is not compatible with bulk item {TargetItem} at {Position}", targetItem, placementPos);
                return false;
            }

            var stackSize = itemResolver.QueryStackSize(targetItem);
            if (stackSize.IsFullStack)
            {
                logger.Verbose("Precondition check failed: Non stackable (or full stack of) bulk item {TargetItem} cannot be placed at non-empty position {Position}", targetItem, placementPos);
                return false;
            }

            var existingStack = itemResolver.QueryStackSize(existingItem);
            if (!stackSize.Merge(existingStack, out var resultStack) ||
                !itemResolver.TryUpdateData(targetItem, resultStack, out var changedItem))
            {
                // cannot merge items
                return false;
            }

            existingItem = changedItem;
            mapData.MarkDirty(placementPos);
            ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForPlacement(targetItem, placementPos));
            return true;
        }

        public bool TryMoveItem<TPosition>(in TItemId item, in TPosition currentPos, in TPosition placementPos)
            where TPosition : IPosition<TPosition>
        {
            if (currentPos.IsInvalid && placementPos.IsInvalid)
            {
                return true;
            }

            if (currentPos.IsInvalid)
            {
                return TryPlaceItem(item, placementPos);
            }

            if (placementPos.IsInvalid)
            {
                return TryRemoveItem(item, currentPos);
            }

            if (item.IsEmpty)
            {
                // Nothing to move.
                return true;
            }

            if (placementPos.LayerId == MapLayer.Indeterminate.LayerId ||
                currentPos.LayerId == MapLayer.Indeterminate.LayerId)
            {
                return false;
            }

            if (!mapContext.TryGetGridDataFor(currentPos.LayerId, out var sourceMapContext))
            {
                logger.Verbose("Unable to resolve grid data for map layer {LayerId} of position {Position}", currentPos.LayerId, currentPos);
                return false;
            }

            if (!sourceMapContext.TryGetWritableView(currentPos.GridZ, out var sourceMap))
            {
                logger.Verbose("Requested grid position for map layer {LayerId} is out of range for position {Position}", currentPos.LayerId, currentPos);
                return false;
            }

            if (!mapContext.TryGetGridDataFor(placementPos.LayerId, out var targetMapContext))
            {
                logger.Verbose("Unable to resolve grid data for map layer {LayerId} of position {Position}", placementPos.LayerId, placementPos);
                return false;
            }

            if (!targetMapContext.TryGetWritableView(placementPos.GridZ, out var targetMap, DataViewCreateMode.CreateMissing))
            {
                logger.Verbose("Requested grid position for map layer {LayerId} is out of range for position {Position}", placementPos.LayerId, placementPos);
                return false;
            }

            var defaultItem = default(TItemId);
            ref var existingItemRef = ref sourceMap.TryGetForUpdate(currentPos.GridX, currentPos.GridY, ref defaultItem, out var success);
            if (!success)
            {
                logger.Verbose("Unable to query map at current position {Position}", currentPos);
                return false;
            }
            Assert.NotNull(existingItemRef);

            ref var targetItemRef = ref targetMap.TryGetForUpdate(placementPos.GridX, placementPos.GridY, ref defaultItem, out success, DataViewCreateMode.CreateMissing);
            if (!success)
            {
                logger.Verbose("Unable to query map at target position {Position}", placementPos);
                return false;
            }
            Assert.NotNull(targetItemRef);

            if (itemIdMetaData.IsReferenceEntity(item))
            {
                if (!equality.Equals(existingItemRef, item))
                {
                    logger.Verbose("Pre-condition failed: Item {ExistingItem} on map position {Position} does not match the given entity {Item}", existingItemRef, currentPos, item);
                    return false;
                }

                if (!targetItemRef.IsEmpty)
                {
                    logger.Verbose("Pre-condition failed: Map position {Position} is not empty", placementPos);
                    return false;
                }

                if (!itemResolver.TryUpdateData(item, new EntityGridPositionUpdateMessage(EntityGridPosition.From(placementPos)), out _))
                {
                    logger.Verbose("Failed to update entity data for {TargetItem} at position {Position}", targetItemRef, placementPos);
                    return false;
                }

                existingItemRef = default;
                targetItemRef = item;
                sourceMapContext.MarkDirty(currentPos);
                targetMapContext.MarkDirty(placementPos);
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForMove(item, currentPos, placementPos));
                return true;
            }

            // are we moving a full stack?
            if (equality.Equals(existingItemRef, item))
            {
                if (targetItemRef.IsEmpty)
                {
                    // the easy path: Moving a full stack to an empty slot.
                    existingItemRef = default;
                    targetItemRef = item;
                    sourceMapContext.MarkDirty(currentPos);
                    targetMapContext.MarkDirty(placementPos);
                    return true;
                }

                if (!itemIdMetaData.IsSameBulkType(item, targetItemRef))
                {
                    logger.Verbose("Precondition failed: Target position is occupied by an incompatible item type");
                    return false;
                }

                var itemStackSize = itemResolver.QueryStackSize(item);
                var targetStackSize = itemResolver.QueryStackSize(targetItemRef);
                if (!targetStackSize.Merge(itemStackSize, out var mergedStack) ||
                    !itemResolver.TryUpdateData(item, mergedStack, out var mergedItem))
                {
                    // the items cannot be place here as there is not enough space in the
                    // target stack to hold all new items.
                    return false;
                }

                existingItemRef = default;
                targetItemRef = mergedItem;
                sourceMapContext.MarkDirty(currentPos);
                targetMapContext.MarkDirty(placementPos);
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForMove(item, currentPos, placementPos));
                return true;
            }
            else
            {
                if (!itemIdMetaData.IsSameBulkType(item, existingItemRef))
                {
                    logger.Verbose("Precondition failed: Source item {ExistingItem} on map does not match the incompatible item type {Item}", existingItemRef, item);
                    return false;
                }

                var itemStackSize = itemResolver.QueryStackSize(item);
                var existingStack = itemResolver.QueryStackSize(existingItemRef);
                var takeResult = existingStack.Take(itemStackSize.Count);
                if (takeResult.NotEnoughItemsInStack)
                {
                    // the stack at the map position does not contain enough items 
                    // to satisfy the requested stack size of targetItem' 
                    return false;
                }

                TItemId changedSourceItem;
                if (takeResult.ItemsLeftInStack.TryGetValue(out var resultStack))
                {
                    if (!itemResolver.TryUpdateData(existingItemRef, resultStack, out changedSourceItem))
                    {
                        // for some unlikely reason we are not allowed to write the resulting stack back 
                        // to the item. 
                        return false;
                    }
                }
                else
                {
                    // The existing stack has been fully consumed. This means we can clear that map position.
                    // Note: That should actually be covered by the Equality test above.
                    changedSourceItem = default;
                }

                if (targetItemRef.IsEmpty)
                {
                    // the target position is empty. so after writing back the changed size for the source,
                    // we can just update the target position to the stack taken.
                    existingItemRef = changedSourceItem;
                    targetItemRef = item;
                    sourceMapContext.MarkDirty(currentPos);
                    targetMapContext.MarkDirty(placementPos);
                    ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForMove(item, currentPos, placementPos));
                    return true;
                }

                if (!itemIdMetaData.IsSameBulkType(item, targetItemRef))
                {
                    logger.Verbose("Precondition failed: Target position is occupied by an incompatible item type");
                    return false;
                }

                var targetStackSize = itemResolver.QueryStackSize(targetItemRef);
                if (!targetStackSize.Merge(itemStackSize, out var mergedStack) ||
                    !itemResolver.TryUpdateData(item, mergedStack, out var mergedItem))
                {
                    // the items cannot be place here as there is not enough space in the
                    // target stack to hold all new items.
                    return false;
                }

                existingItemRef = changedSourceItem;
                targetItemRef = mergedItem;
                sourceMapContext.MarkDirty(currentPos);
                targetMapContext.MarkDirty(placementPos);
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForMove(item, currentPos, placementPos));
                return true;
            }
        }

        public bool TrySwapItem<TPosition>(in TItemId sourceItem, in TPosition sourcePosition, in TItemId targetItem, in TPosition targetPosition)
            where TPosition : IPosition<TPosition>
        {
            if (sourcePosition.IsInvalid && targetPosition.IsInvalid)
            {
                return true;
            }

            if (sourcePosition.IsInvalid || targetPosition.IsInvalid)
            {
                return false;
            }

            if (sourceItem.IsEmpty && targetItem.IsEmpty)
            {
                return true;
            }

            if (sourcePosition.LayerId == MapLayer.Indeterminate.LayerId ||
                targetPosition.LayerId == MapLayer.Indeterminate.LayerId)
            {
                return false;
            }

            if (sourceItem.IsEmpty)
            {
                return TryMoveItem(targetItem, targetPosition, sourcePosition);
            }

            if (targetItem.IsEmpty)
            {
                return TryMoveItem(sourceItem, sourcePosition, targetPosition);
            }

            // a combined remove from currentPos and subsequence move to placement pos
            if (!mapContext.TryGetGridDataFor(sourcePosition.LayerId, out var sourceMapContext))
            {
                logger.Verbose("Unable to resolve grid data for map layer {LayerId} of position {Position}", sourcePosition.LayerId, sourcePosition);
                return false;
            }

            if (!sourceMapContext.TryGetWritableView(sourcePosition.GridZ, out var sourceMap))
            {
                logger.Verbose("Requested grid position for map layer {LayerId} is out of range for position {Position}", sourcePosition.LayerId, sourcePosition);
                return false;
            }

            if (!mapContext.TryGetGridDataFor(targetPosition.LayerId, out var targetMapContext))
            {
                logger.Verbose("Unable to resolve grid data for map layer {LayerId} of position {Position}", targetPosition.LayerId, targetPosition);
                return false;
            }

            if (!targetMapContext.TryGetWritableView(targetPosition.GridZ, out var targetMap, DataViewCreateMode.CreateMissing))
            {
                logger.Verbose("Requested grid position for map layer {LayerId} is out of range for position {Position}", targetPosition.LayerId, targetPosition);
                return false;
            }

            var defaultItem = default(TItemId);
            ref var sourceMapItem = ref sourceMap.TryGetForUpdate(sourcePosition.GridX, sourcePosition.GridY, ref defaultItem, out var success);
            if (!success)
            {
                logger.Verbose("Unable to query map at current position {Position}", sourcePosition);
                return false;
            }

            ref var targetMapItem = ref targetMap.TryGetForUpdate(targetPosition.GridX, targetPosition.GridY, ref defaultItem, out success);
            if (!success)
            {
                logger.Verbose("Unable to query map at target position {Position}", targetPosition);
                return false;
            }

            Assert.NotNull(sourceMapItem);
            Assert.NotNull(targetMapItem);
            
            var sourceStackSize = itemResolver.QueryStackSize(sourceItem);
            var targetStackSize = itemResolver.QueryStackSize(targetItem);

            var isExactSourceCell = equality.Equals(sourceItem, sourceMapItem);
            var isExactTargetCell = equality.Equals(targetItem, targetMapItem);
            if (isExactSourceCell || isExactTargetCell)
            {
                // if either item is a reference item, a non-stackable bulk item or a full stack, we have to move the entire map cell. 
                // This strict condition is luckily easy to test for.

                if (!isExactSourceCell)
                {
                    logger.Verbose("Precondition failed: Source item is not found at map position {SourcePosition}", sourcePosition);
                    return false;
                }

                if (!isExactTargetCell)
                {
                    logger.Verbose("Precondition failed: Target item is not found at map position {TargetPosition}", targetPosition);
                    return false;
                }

                if (!itemResolver.TryUpdateData(targetItem, EntityGridPositionUpdateMessage.From(sourcePosition), out _))
                {
                    logger.Verbose("Execution failed: Unable to update entity data for target item {TargetItem}", targetItem);
                    return false;
                }

                if (!itemResolver.TryUpdateData(sourceItem, EntityGridPositionUpdateMessage.From(targetPosition), out _))
                {
                    logger.Verbose("Execution failed: Unable to update entity data for source item {SourceItem}", sourceItem);
                    return false;
                }

                sourceMapItem = targetItem;
                targetMapItem = sourceItem;
                sourceMapContext.MarkDirty(sourcePosition);
                targetMapContext.MarkDirty(targetPosition);
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForMove(sourceItem, sourcePosition, targetPosition));
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForMove(targetItem, targetPosition, sourcePosition));
                return true;
            }

            // Stackable bulk items.
            if (!itemIdMetaData.IsSameBulkType(sourceItem, targetItem) ||
                !itemIdMetaData.IsSameBulkType(sourceItem, sourceMapItem) ||
                !itemIdMetaData.IsSameBulkType(targetItem, targetMapItem))
            {
                logger.Verbose("Precondition failed: Can only swap partial stacks of items between compatible item types");
                return false;
            }

            var sourceMapStackSize = itemResolver.QueryStackSize(sourceMapItem);
            var targetMapStackSize = itemResolver.QueryStackSize(targetMapItem);

            var sourceMapResultStack = sourceMapStackSize.Count - sourceStackSize.Count + targetStackSize.Count;
            var targetMapResultStack = targetMapStackSize.Count - targetStackSize.Count + sourceStackSize.Count;

            if (sourceMapResultStack < 0 || sourceMapResultStack > sourceStackSize.MaximumStackSize)
            {
                logger.Verbose("Precondition failed: Source item type would exceed stack size");
                return false;
            }

            if (targetMapResultStack < 0 || targetMapResultStack > targetStackSize.MaximumStackSize)
            {
                logger.Verbose("Precondition failed: Target item type would exceed stack size");
                return false;
            }

            var sourceResultStack = targetStackSize.WithCount(sourceMapResultStack);
            var targetResultStack = targetStackSize.WithCount(targetMapResultStack);

            if (itemResolver.TryUpdateData(sourceMapItem, sourceResultStack, out var sourceResultEntity) &&
                itemResolver.TryUpdateData(targetMapItem, targetResultStack, out var targetResultEntity))
            {
                sourceMapItem = sourceResultEntity;
                targetMapItem = targetResultEntity;
                sourceMapContext.MarkDirty(sourcePosition);
                targetMapContext.MarkDirty(targetPosition);
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForMove(sourceItem, sourcePosition, targetPosition));
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForMove(targetItem, targetPosition, sourcePosition));
                return true;
            }

            return false;
        }
    }
}
