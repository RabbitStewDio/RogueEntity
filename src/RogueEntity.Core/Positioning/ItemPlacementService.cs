using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using Serilog;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Positioning
{
    public class ItemPlacementService<TItemId> : IItemPlacementService<TItemId>
        where TItemId : struct, IEntityKey
    {
        static readonly EqualityComparer<TItemId> equalityComparer = EqualityComparer<TItemId>.Default;
        static readonly ILogger logger = SLog.ForContext<ItemPlacementService<TItemId>>();
        readonly IMapContext<TItemId> mapContext;
        readonly IItemResolver<TItemId> itemResolver;

        public ItemPlacementService(IItemResolver<TItemId> itemResolver, IMapContext<TItemId> mapContext)
        {
            this.mapContext = mapContext;
            this.itemResolver = itemResolver;
        }

#pragma warning disable CS0067
        public event EventHandler<ItemPositionChangedEvent<TItemId>>? ItemPositionChanged;
#pragma warning restore CS0067

        public bool TryQueryItem<TPosition>(in TPosition placementPos, out TItemId item)
            where TPosition : struct, IPosition<TPosition>
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

            if (!mapContext.TryGetMapDataFor(placementPos.LayerId, out var mapData))
            {
                logger.Verbose("Unable to resolve grid data for map layer {LayerId} of position {Position}", placementPos.LayerId, placementPos);
                item = default;
                return false;
            }

            using var buffer = BufferListPool<TItemId>.GetPooled();
            foreach (var b in mapData.QueryItem(placementPos, buffer))
            {
                item = b;
                return true;
            }

            item = default;
            return true;
        }

        public BufferList<TItemId> QueryItems<TPosition>(in TPosition placementPos, BufferList<TItemId>? buffer = null)
            where TPosition : struct, IPosition<TPosition>
        {
            buffer = BufferList.PrepareBuffer(buffer);
            if (placementPos.IsInvalid)
            {
                logger.Verbose("Given position {Position} is invalid", placementPos);
                return buffer;
            }

            if (placementPos.LayerId == MapLayer.Indeterminate.LayerId)
            {
                return buffer;
            }

            if (!mapContext.TryGetMapDataFor(placementPos.LayerId, out var mapData))
            {
                logger.Verbose("Unable to resolve grid data for map layer {LayerId} of position {Position}", placementPos.LayerId, placementPos);
                return buffer;
            }

            mapData.QueryItem(placementPos, buffer);
            return buffer;
        }

        public bool TryRemoveItem<TPosition>(in TItemId targetItem, in TPosition placementPos)
            where TPosition : struct, IPosition<TPosition>
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

            if (!mapContext.TryGetMapDataFor(placementPos.LayerId, out var map))
            {
                logger.Verbose("Unable to resolve grid data for map layer {LayerId} of position {Position}", placementPos.LayerId, placementPos);
                return false;
            }

            using var buffer = BufferListPool<TItemId>.GetPooled();
            foreach (var item in map.QueryItem(placementPos, buffer))
            {
                // see whether there is an item that directly matches the given item-id
                if (equalityComparer.Equals(item, targetItem) && map.TryRemoveItem(item, placementPos))
                {
                    if (!itemResolver.TryUpdateData(targetItem, Position.Invalid, out _))
                    {
                        logger.Warning("Unable to update entity data for position update on entity {ExistingItem}", targetItem);
                    }

                    map.MarkDirty(placementPos);
                    ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForRemove(targetItem, placementPos));
                    return true;
                }
            }

            var stackSize = itemResolver.QueryStackSize(targetItem);
            if (stackSize.MaximumStackSize == 1)
            {
                return false;
            }

            // Not so easy: Bulk items require special handling as they can be stackable.
            // The item given will be subsumed into an existing stack.
            foreach (var existingItem in map.QueryItem(placementPos, buffer))
            {
                if (!itemResolver.IsSameStackType(targetItem, existingItem))
                {
                    logger.Verbose("Safety check failed: Unable to locate the correct bulk item type for {ExistingItem} at position {Position}", targetItem, placementPos);
                    continue;
                }

                var existingStack = itemResolver.QueryStackSize(existingItem);
                var takeResult = existingStack.Take(stackSize.Count);
                if (takeResult.ItemsNotAvailableInStack != 0)
                {
                    // the stack at the map position does not contain enough items 
                    // to satisfy the requested stack size. This is not something we would want to handle here 
                    continue;
                }

                if (!takeResult.ItemsLeftInStack.TryGetValue(out var resultStack))
                {
                    // No left over stack means all items have been removed from the stack. 
                    // this means we just need to remove the existing map item completely.
                    if (!map.TryRemoveItem(existingItem, placementPos))
                    {
                        if (!itemResolver.TryUpdateData(existingItem, Position.Invalid, out _))
                        {
                            logger.Warning("Unable to update entity data for position update on entity {ExistingItem}", targetItem);
                        }

                        map.MarkDirty(placementPos);
                        ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForRemove(existingItem, placementPos));
                        itemResolver.Destroy(existingItem);
                        return true;
                    }

                    // this here means the map update failed.
                    logger.Verbose("Unable to remove item {ChangedItem} at position {Position}", targetItem, placementPos);
                    return false;
                }

                // Handle the case where we do not remove all items from the stack
                // we need to remove the original item and then re-add the left over items.
                if (!itemResolver.TryUpdateData(existingItem, resultStack, out var changedItem))
                {
                    // for some unlikely reason we are not allowed to write the resulting stack back 
                    // to the item. 
                    //
                    // Either way we are done here, we cannot proceed.
                    continue;
                }

                if (!map.TryUpdateItem(existingItem, changedItem, placementPos))
                {
                    logger.Verbose("Unable to return remaining stack of item {ChangedItem} at position {Position}", changedItem, placementPos);
                    // we removed the item, even though we cannot reinsert the changed item,
                    // it is still necessary to fire the change event.
                    map.MarkDirty(placementPos);
                    ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForRemove(targetItem, placementPos));
                    return false;
                }

                map.MarkDirty(placementPos);
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForRemove(targetItem, placementPos));
                return true;
            }

            return false;
        }

        public bool TryPlaceItem<TPosition>(in TItemId targetItem, in TPosition placementPos)
            where TPosition : struct, IPosition<TPosition>
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

            if (!mapContext.TryGetMapDataFor(placementPos.LayerId, out var map))
            {
                logger.Verbose("Unable to resolve grid data for map layer {LayerId} of position {Position}", placementPos.LayerId, placementPos);
                return false;
            }

            var stackSize = itemResolver.QueryStackSize(targetItem);

            if (TryFindTargetItem(map, placementPos, targetItem, stackSize, out var existingItem))
            {
                // there exists a compatible item in the map position and it has enough space to take
                // on the required elements
                var existingStack = itemResolver.QueryStackSize(existingItem);
                if (!existingStack.Merge(stackSize, out var mergedStack))
                {
                    // not enough space to merge the stacks. 
                    // This really should never happen at this point, as we prequalified the item in TryFindItem
                    logger.Verbose("Unable to merge existing entity data for stack size update on entity {ExistingItem}", targetItem);
                    return false;
                }

                if (!itemResolver.TryUpdateData(existingItem, mergedStack, out var changedItem))
                {
                    logger.Verbose("Unable to update stack of existing entity data for stack size update on entity {ExistingItem}", targetItem);
                    // it should be unlikely for maps to fail on updates, but in case they do
                    // we need to tell everyone that the map item got removed in the previous step.
                    return false;
                }

                if (!map.TryUpdateItem(existingItem, changedItem, placementPos))
                {
                    logger.Warning("Unable to update entity data for position update on entity {ExistingItem}", targetItem);
                    return false;
                }

                map.MarkDirty(placementPos);
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForRemove(targetItem, placementPos));
                if (!equalityComparer.Equals(targetItem, existingItem))
                {
                    itemResolver.Destroy(targetItem);
                }

                return true;
            }

            if (!existingItem.IsEmpty && !map.AllowMultipleItems)
            {
                logger.Warning("Unable to place entity {ExistingItem}; placement position is blocked", targetItem);
                return false;
            }

            // the tile is either empty or it allows multiple items on the same position

            if (!map.TryInsertItem(targetItem, placementPos))
            {
                // we removed the item, even though we cannot reinsert the changed item,
                // it is still necessary to fire the change event.
                logger.Verbose("Unable to return remaining stack of item {ChangedItem} at position {Position}", targetItem, placementPos);
                map.MarkDirty(placementPos);
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForPlacement(targetItem, placementPos));
                return false;
            }

            if (!itemResolver.TryUpdateData(targetItem, Position.From(placementPos), out _))
            {
                logger.Warning("Unable to update entity data for position update on entity {ExistingItem}", targetItem);
            }

            map.MarkDirty(placementPos);
            ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForPlacement(targetItem, placementPos));
            return true;
        }

        public bool TryMoveItem<TPosition>(in TItemId item, in TPosition currentPos, in TPosition placementPos)
            where TPosition : struct, IPosition<TPosition>
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

            if (!mapContext.TryGetMapDataFor(currentPos.LayerId, out var sourceMapContext))
            {
                logger.Verbose("Unable to resolve grid data for map layer {LayerId} of position {Position}", currentPos.LayerId, currentPos);
                return false;
            }

            if (!mapContext.TryGetMapDataFor(placementPos.LayerId, out var targetMapContext))
            {
                logger.Verbose("Unable to resolve grid data for map layer {LayerId} of position {Position}", placementPos.LayerId, placementPos);
                return false;
            }

            var stack = itemResolver.QueryStackSize(item);
            bool sourceItemFound = TryFindSourceItem(sourceMapContext, currentPos, item, stack, out var existingItemRef);
            if (!sourceItemFound)
            {
                logger.Verbose("Unable to find requested item at current position {Position}", currentPos);
                return false;
            }

            bool targetPosFound = TryFindTargetItem(targetMapContext, placementPos, item, stack, out var targetItemRef);

            // target item is an existing stack that can receive additional items
            if (targetPosFound)
            {
                var targetStack = itemResolver.QueryStackSize(targetItemRef);
                if (!targetStack.Merge(stack, out var mergedStack))
                {
                    // should not happen
                    logger.Verbose("Precondition failed: Target item stack would exceed stack size");
                    return false;
                }

                // source item is an exact match
                if (equalityComparer.Equals(existingItemRef, item))
                {
                    // found a full match
                    // remove the source item
                    // add it to the target 
                    if (!sourceMapContext.TryRemoveItem(item, currentPos))
                    {
                        logger.Verbose("Unable to remove source item");
                        return false;
                    }

                    if (!itemResolver.TryUpdateData(targetItemRef, mergedStack, out var changedItem))
                    {
                        logger.Verbose("Unable to update target item stack");
                        return false;
                    }

                    if (!targetMapContext.TryUpdateItem(targetItemRef, changedItem, placementPos))
                    {
                        logger.Verbose("Unable to update target item");
                        return false;
                    }

                    if (!itemResolver.TryUpdateData(existingItemRef, Position.Invalid, out _))
                    {
                        logger.Verbose("Notification failed: Unable to update entity data for source item {SourceItem}", existingItemRef);
                    }
                    
                    if (!itemResolver.TryUpdateData(changedItem, Position.From(placementPos), out _))
                    {
                        logger.Verbose("Notification failed: Unable to update entity data for source item {SourceItem}", existingItemRef);
                    }
                    
                    sourceMapContext.MarkDirty(currentPos);
                    targetMapContext.MarkDirty(placementPos);
                    ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForRemove(existingItemRef, currentPos));
                    ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForMove(changedItem, currentPos, placementPos));
                }
                else
                {
                    // Source item is a partial match. Removing a partial stack means we need to update
                    // both source and target item stacks.
                    var sourceStack = itemResolver.QueryStackSize(existingItemRef);
                    var takeResult = sourceStack.Take(stack.Count);
                    if (takeResult.NotEnoughItemsInStack || !takeResult.ItemsLeftInStack.TryGetValue(out var reducedSourceStack))
                    {
                        // both of these conditions should have been checked earlier
                        logger.Verbose("Unable to reduce source stack");
                        return false;
                    }

                    if (!itemResolver.TryUpdateData(existingItemRef, reducedSourceStack, out var reducedItemRef))
                    {
                        logger.Verbose("Unable to update target source stack");
                        return false;
                    }

                    if (!itemResolver.TryUpdateData(targetItemRef, mergedStack, out var changedTargetItem))
                    {
                        logger.Verbose("Unable to update target item stack");
                        return false;
                    }

                    if (!sourceMapContext.TryUpdateItem(existingItemRef, reducedItemRef, currentPos))
                    {
                        logger.Verbose("Unable to update source item on map");
                        return false;
                    }
                    
                    if (!targetMapContext.TryUpdateItem(targetItemRef, changedTargetItem, placementPos))
                    {
                        logger.Verbose("Unable to update target item on map");
                        return false;
                    }
                    
                    if (!itemResolver.TryUpdateData(reducedItemRef, Position.From(currentPos), out _))
                    {
                        logger.Verbose("Notification failed: Unable to update entity data for source item {SourceItem}", existingItemRef);
                    }
                    
                    if (!itemResolver.TryUpdateData(changedTargetItem, Position.From(placementPos), out _))
                    {
                        logger.Verbose("Notification failed: Unable to update entity data for source item {SourceItem}", existingItemRef);
                    }
                    
                    sourceMapContext.MarkDirty(currentPos);
                    targetMapContext.MarkDirty(placementPos);
                    ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForRemove(reducedItemRef, currentPos));
                    ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForMove(changedTargetItem, currentPos, placementPos));
                }
                
                return true;
            }
            
            if (!targetItemRef.IsEmpty && !targetMapContext.AllowMultipleItems)
            {
                logger.Verbose("Unable to find space for item at target position {Position}", placementPos);
                return false;
            }

            // there has been no matching item at the target position that can accept the removed item
            // We can generate a new item stack at the target position.
            if (equalityComparer.Equals(existingItemRef, item))
            {
                // found a full match
                // remove the source item
                // add it to the target 
                if (!sourceMapContext.TryRemoveItem(item, currentPos))
                {
                    logger.Verbose("Unable to remove source item");
                    return false;
                }

                if (!targetMapContext.TryInsertItem(item, placementPos))
                {
                    logger.Verbose("Unable to update target item");
                    return false;
                }

                if (!itemResolver.TryUpdateData(item, Position.From(placementPos), out _))
                {
                    logger.Verbose("Execution failed: Unable to update entity data for source item {SourceItem}", existingItemRef);
                }
                    
                sourceMapContext.MarkDirty(currentPos);
                targetMapContext.MarkDirty(placementPos);
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForMove(item, currentPos, placementPos));
            }
            else
            {
                // Source item is a partial match. Removing a partial stack means we need to update
                // both source and target item stacks.
                var sourceStack = itemResolver.QueryStackSize(existingItemRef);
                var takeResult = sourceStack.Take(stack.Count);
                if (takeResult.NotEnoughItemsInStack || !takeResult.ItemsLeftInStack.TryGetValue(out var reducedSourceStack))
                {
                    // both of these conditions should have been checked earlier
                    logger.Verbose("Unable to reduce source stack");
                    return false;
                }

                if (!itemResolver.TryUpdateData(existingItemRef, reducedSourceStack, out var reducedItemRef))
                {
                    logger.Verbose("Unable to update target source stack");
                    return false;
                }

                if (!sourceMapContext.TryUpdateItem(existingItemRef, reducedItemRef, currentPos))
                {
                    logger.Verbose("Unable to update source item on map");
                    return false;
                }

                if (!targetMapContext.TryInsertItem(item, placementPos))
                {
                    logger.Verbose("Unable to update target item");
                    return false;
                }

                sourceMapContext.MarkDirty(currentPos);
                targetMapContext.MarkDirty(placementPos);
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForRemove(reducedItemRef, currentPos));
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForPlacement(item, placementPos));
            }

            return true;
        }

        public bool TrySwapItem<TPosition>(in TItemId sourceItem, in TPosition sourcePosition, in TItemId targetItem, in TPosition targetPosition)
            where TPosition : struct, IPosition<TPosition>
        {
            if (sourcePosition.IsInvalid && targetPosition.IsInvalid)
            {
                logger.Verbose("Source and target are both invalid; indicating success (No-Op)");
                return true;
            }

            if (sourcePosition.IsInvalid || targetPosition.IsInvalid)
            {
                logger.Verbose("Either Source or target are invalid; indicating failure (Invalid)");
                return false;
            }

            if (sourceItem.IsEmpty && targetItem.IsEmpty)
            {
                logger.Verbose("Source and target are empty; indicating success (No-Op)");
                return true;
            }

            if (sourcePosition.LayerId == MapLayer.Indeterminate.LayerId ||
                targetPosition.LayerId == MapLayer.Indeterminate.LayerId)
            {
                logger.Verbose("Source and target have indeterminate layer; indicating failure (No-Op)");
                return false;
            }

            if (sourceItem.IsEmpty)
            {
                logger.Verbose("Source item is empty; remapping into ordinary move");
                return TryMoveItem(targetItem, targetPosition, sourcePosition);
            }

            if (targetItem.IsEmpty)
            {
                logger.Verbose("Target item is empty; remapping into ordinary move");
                return TryMoveItem(sourceItem, sourcePosition, targetPosition);
            }

            // a combined remove from currentPos and subsequence move to placement pos
            if (!mapContext.TryGetMapDataFor(sourcePosition.LayerId, out var sourceMap))
            {
                logger.Verbose("Unable to resolve grid data for map layer {LayerId} of position {Position}", sourcePosition.LayerId, sourcePosition);
                return false;
            }

            if (!mapContext.TryGetMapDataFor(targetPosition.LayerId, out var targetMap))
            {
                logger.Verbose("Unable to resolve grid data for map layer {LayerId} of position {Position}", sourcePosition.LayerId, sourcePosition);
                return false;
            }

            // at this point we have established that this is a proper swap
            // first, lets find the items in question. If either fails, there is no point continuing
            var sourceStackSize = itemResolver.QueryStackSize(sourceItem);
            var targetStackSize = itemResolver.QueryStackSize(targetItem);

            // at this point we know that there are indeed compatible items at those map cells
            // and those items found contain a stack larger enough for the requested operation

            if (!TryFindSourceItem(sourceMap, sourcePosition, sourceItem, sourceStackSize, out var sourceItemAtMap))
            {
                logger.Verbose("Unable to find source item {SourceItem} at position {Position}", sourceItem, sourcePosition);
                return false;
            }

            if (!TryFindSourceItem(targetMap, targetPosition, targetItem, targetStackSize, out var targetItemAtMap))
            {
                logger.Verbose("Unable to find target item {SourceItem} at position {Position}", sourceItem, sourcePosition);
                return false;
            }

            var isExactSourceCell = equalityComparer.Equals(sourceItem, sourceItemAtMap);
            var isExactTargetCell = equalityComparer.Equals(targetItem, targetItemAtMap);
            if (isExactSourceCell || isExactTargetCell)
            {
                // if either item is a reference item, a non-stackable bulk item or a full stack, we have to move the entire map cell. 
                // This strict condition is luckily easy to test for.
                if (!isExactSourceCell)
                {
                    logger.Verbose("Precondition failed: Exact Source item is not found at map position {SourcePosition}", sourcePosition);
                    return false;
                }

                if (!isExactTargetCell)
                {
                    logger.Verbose("Precondition failed: Exact Target item is not found at map position {TargetPosition}", targetPosition);
                    return false;
                }

                if (!itemResolver.TryUpdateData(targetItem, Position.From(sourcePosition), out _))
                {
                    logger.Verbose("Execution failed: Unable to update entity data for target item {TargetItem}", targetItem);
                    return false;
                }

                if (!itemResolver.TryUpdateData(sourceItem, Position.From(targetPosition), out _))
                {
                    logger.Verbose("Execution failed: Unable to update entity data for source item {SourceItem}", sourceItem);
                    return false;
                }

                if (!sourceMap.TryRemoveItem(sourceItemAtMap, sourcePosition) ||
                    !targetMap.TryRemoveItem(targetItemAtMap, targetPosition) ||
                    !sourceMap.TryInsertItem(targetItemAtMap, sourcePosition) ||
                    !targetMap.TryInsertItem(sourceItemAtMap, targetPosition))
                {
                    logger.Verbose("Execution failed: Unable to swap entity data for source item {SourceItem} and target item {TargetItem}", sourceItem, targetItem);
                    return false;
                }

                sourceMap.MarkDirty(sourcePosition);
                targetMap.MarkDirty(targetPosition);
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForMove(sourceItem, sourcePosition, targetPosition));
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForMove(targetItem, targetPosition, sourcePosition));
                return true;
            }

            // Complex case: The items in question are both stackable. 
            if (!itemResolver.IsSameStackType(sourceItem, targetItem) ||
                !itemResolver.IsSameStackType(sourceItem, sourceItemAtMap) ||
                !itemResolver.IsSameStackType(targetItem, targetItemAtMap))
            {
                logger.Verbose("Precondition failed: Can only swap partial stacks of items between compatible item types");
                return false;
            }

            var sourceMapStackSize = itemResolver.QueryStackSize(sourceItemAtMap);
            var targetMapStackSize = itemResolver.QueryStackSize(targetItemAtMap);

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

            if (itemResolver.TryUpdateData(sourceItemAtMap, sourceResultStack, out var sourceResultEntity) &&
                itemResolver.TryUpdateData(targetItemAtMap, targetResultStack, out var targetResultEntity) &&
                sourceMap.TryUpdateItem(sourceItemAtMap, sourceResultEntity, sourcePosition) &&
                targetMap.TryUpdateItem(targetItemAtMap, targetResultEntity, targetPosition))
            {
                sourceMap.MarkDirty(sourcePosition);
                targetMap.MarkDirty(targetPosition);
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForMove(sourceItem, sourcePosition, targetPosition));
                ItemPositionChanged?.Invoke(this, ItemPositionChangedEvent.ForMove(targetItem, targetPosition, sourcePosition));
                return true;
            }

            return false;
        }

        /// <summary>
        ///    Tries to find an item matching the same type as the given matchItem, while also checking that this
        ///    item has enough space. The result item is always filled with the last item on that map position.
        ///    The result itemId will be empty (default(TItemId)) when the map cell is empty.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="position"></param>
        /// <param name="matchItem"></param>
        /// <param name="requiredStackSize"></param>
        /// <param name="result"></param>
        /// <typeparam name="TPosition"></typeparam>
        /// <returns>true if an item is found that has enough stack space to accommodate the given requiredStackSize</returns>
        bool TryFindTargetItem<TPosition>(IMapDataContext<TItemId> map,
                                          TPosition position,
                                          TItemId matchItem,
                                          StackCount requiredStackSize,
                                          out TItemId result)
            where TPosition : struct, IPosition<TPosition>
        {
            result = default;
            using var buffer = BufferListPool<TItemId>.GetPooled();
            foreach (var item in map.QueryItem(position, buffer))
            {
                result = item;
                if (!itemResolver.IsSameStackType(matchItem, item))
                {
                    continue;
                }

                var existingStack = itemResolver.QueryStackSize(item);
                if (!existingStack.Merge(requiredStackSize, out _))
                {
                    // not enough space to merge the stacks. 
                    // maybe there is a better item in the list of items elsewhere
                    continue;
                }

                return true;
            }

            return false;
        }

        bool TryFindSourceItem<TPosition>(IMapDataContext<TItemId> map,
                                          TPosition position,
                                          TItemId matchItem,
                                          StackCount requiredStackSize,
                                          out TItemId result)
            where TPosition : struct, IPosition<TPosition>
        {
            result = default;
            var inexactMatch = default(TItemId);
            using var buffer = BufferListPool<TItemId>.GetPooled();
            foreach (var item in map.QueryItem(position, buffer))
            {
                result = item;
                if (equalityComparer.Equals(matchItem, item))
                {
                    return true;
                }

                if (!itemResolver.IsSameStackType(matchItem, item))
                {
                    continue;
                }

                var existingStack = itemResolver.QueryStackSize(item);
                if (existingStack.Count == requiredStackSize.Count)
                {
                    // found a perfect match
                    return true;
                }

                if (existingStack.Count > requiredStackSize.Count)
                {
                    inexactMatch = item;
                }
            }

            if (inexactMatch.IsEmpty)
            {
                return false;
            }

            result = inexactMatch;
            return true;
        }
    }
}