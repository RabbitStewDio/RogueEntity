using System;
using System.Collections.Generic;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;
using Rectangle = RogueEntity.Core.Utils.Rectangle;
using RectangleRange = RogueEntity.Core.Utils.RectangleRange;

namespace RogueEntity.Core.Positioning.Grid
{
    public class GridItemPlacementService<TGameContext, TItemId> : IItemPlacementService<TGameContext, TItemId>
        where TGameContext : IGridMapContext<TItemId>, IItemContext<TGameContext, TItemId>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        static readonly EqualityComparer<TItemId> Equality = EqualityComparer<TItemId>.Default;
        static readonly ILogger logger = SLog.ForContext<GridItemPlacementService<TGameContext, TItemId>>();
        readonly MapLayerLookupDelegate mapLayerResolver;

        public GridItemPlacementService(MapLayerLookupDelegate mapLayerResolver)
        {
            this.mapLayerResolver = mapLayerResolver ?? throw new ArgumentNullException(nameof(mapLayerResolver));
        }

        public bool TryFindAvailableItemSlot(TGameContext context, TItemId itemToBePlaced, in Position origin, out Position placementPos, int searchRadius = 10)
        {
            if (origin.IsInvalid)
            {
                logger.Verbose("Given position {position} is invalid", origin);
                placementPos = default;
                return false;
            }

            if (!mapLayerResolver(origin.LayerId, out var mapLayer) ||
                !context.TryGetGridDataFor(mapLayer, out var mapData))
            {
                logger.Verbose("Unable to resolve grid data for map layer {p.LayerId} of position {position}", origin.LayerId, origin);
                placementPos = default;
                return false;
            }

            if (!mapData.IsValid(origin) ||
                !mapData.TryGetWritableView(origin.GridZ, out var map, DataViewCreateMode.CreateMissing))
            {
                logger.Verbose("Requested grid position for map layer {p.LayerId} is out of range", origin.LayerId, origin);
                placementPos = default;
                return false;
            }

            var itemStack = context.ItemResolver.QueryStackSize(itemToBePlaced, context);
            var gx = origin.GridX;
            var gy = origin.GridY;

            for (int r = 0; r < searchRadius; r += 1)
            {
                foreach (var c in new RectangleRange(new Rectangle(gx, gy, searchRadius, searchRadius)))
                {
                    if (c.X < 0 || c.Y < 0) continue;

                    placementPos = origin.From(c.X, c.Y);
                    if (!mapData.IsValid(placementPos))
                    {
                        continue;
                    }

                    var itemAtPos = map[c.X, c.Y];
                    if (Equality.Equals(itemAtPos, default))
                    {
                        return true;
                    }

                    if (!itemAtPos.IsSameBulkType(itemToBePlaced))
                    {
                        continue;
                    }

                    var stackSize = context.ItemResolver.QueryStackSize(itemAtPos, context);
                    if (itemStack.Count + stackSize.Count < stackSize.MaximumStackSize)
                    {
                        return true;
                    }
                }
            }

            placementPos = default;
            return false;
        }

        public bool TryFindEmptyItemSlot(TGameContext context, in Position origin, out Position placementPos, int searchRadius = 10)
        {
            if (origin.IsInvalid)
            {
                logger.Verbose("Given position {position} is invalid", origin);
                placementPos = default;
                return false;
            }

            if (!mapLayerResolver(origin.LayerId, out var mapLayer) ||
                !context.TryGetGridDataFor(mapLayer, out var mapData))
            {
                logger.Verbose("Unable to resolve grid data for map layer {p.LayerId} of position {position}", origin.LayerId, origin);
                placementPos = default;
                return false;
            }

            if (!mapData.IsValid(origin) ||
                !mapData.TryGetWritableView(origin.GridZ, out var map, DataViewCreateMode.CreateMissing))
            {
                logger.Verbose("Requested grid position for map layer {p.LayerId} is out of range", origin.LayerId, origin);
                placementPos = default;
                return false;
            }

            var gx = origin.GridX;
            var gy = origin.GridY;

            for (int r = 0; r < searchRadius; r += 1)
            {
                foreach (var c in new RectangleRange(new Rectangle(gx, gy, searchRadius, searchRadius)))
                {
                    if (c.X < 0 || c.Y < 0) continue;

                    placementPos = origin.From(c.X, c.Y);
                    if (!mapData.IsValid(placementPos))
                    {
                        continue;
                    }

                    if (Equality.Equals(map[c.X, c.Y], default))
                    {
                        return true;
                    }
                }
            }

            placementPos = default;
            return false;
        }

        public bool TryRemoveItem(TGameContext context, in TItemId targetItem, in Position placementPos, bool forcePlacement = false)
        {
            if (placementPos.IsInvalid)
            {
                logger.Verbose("Given position {position} is invalid", placementPos);
                return false;
            }

            if (!mapLayerResolver(placementPos.LayerId, out var mapLayer) ||
                !context.TryGetGridDataFor(mapLayer, out var mapData))
            {
                logger.Verbose("Unable to resolve grid data for map layer {p.LayerId} of position {position}", placementPos.LayerId, placementPos);
                return false;
            }

            if (!mapData.IsValid(placementPos) ||
                !mapData.TryGetWritableView(placementPos.GridZ, out var map, DataViewCreateMode.CreateMissing))
            {
                logger.Verbose("Requested grid position for map layer {p.LayerId} is out of range", placementPos.LayerId, placementPos);
                return false;
            }

            var existingItem = map[placementPos.GridX, placementPos.GridY];
            if (targetItem.IsReference || !targetItem.IsSameBulkType(existingItem))
            {
                return TryReplaceItemInternal(context, targetItem, default, placementPos, forcePlacement, map, mapData);
            }

            var stackSize = context.ItemResolver.QueryStackSize(targetItem, context);
            if (stackSize.MaximumStackSize == 1)
            {
                return TryReplaceItemInternal(context, targetItem, default, placementPos, forcePlacement, map, mapData);
            }

            var existingStack = context.ItemResolver.QueryStackSize(existingItem, context);
            existingStack.Take(stackSize.Count, out var resultStack, out var remainToBeTaken);
            if (remainToBeTaken != 0 || 
                !context.ItemResolver.TryUpdateData(targetItem, context, resultStack, out var changedItem))
            {
                // the stack at the map position does not contain enough items 
                // to satisfy the requested stack size of targetItem' 
                // or for some unlikely reason we are not allowed to write the resulting stack back 
                // to the item. 
                //
                // Either way we are done here, we cannot proceed.
                return false;
            }
            
            return TryReplaceItemInternal(context, existingItem, changedItem, placementPos, forcePlacement, map, mapData);
        }

        public bool TryPlaceItem(TGameContext context, in TItemId targetItem, in Position placementPos, bool forcePlacement = false)
        {
            if (placementPos.IsInvalid)
            {
                logger.Verbose("Given position {position} is invalid", placementPos);
                return false;
            }

            if (!mapLayerResolver(placementPos.LayerId, out var mapLayer) ||
                !context.TryGetGridDataFor(mapLayer, out var mapData))
            {
                logger.Verbose("Unable to resolve grid data for map layer {p.LayerId} of position {position}", placementPos.LayerId, placementPos);
                return false;
            }

            if (!mapData.IsValid(placementPos) ||
                !mapData.TryGetWritableView(placementPos.GridZ, out var map, DataViewCreateMode.CreateMissing))
            {
                logger.Verbose("Requested grid position for map layer {p.LayerId} is out of range", placementPos.LayerId, placementPos);
                return false;
            }

            var existingItem = map[placementPos.GridX, placementPos.GridY];
            if (targetItem.IsReference || !targetItem.IsSameBulkType(existingItem))
            {
                return TryReplaceItemInternal(context, default, targetItem, placementPos, forcePlacement, map, mapData);
            }

            var stackSize = context.ItemResolver.QueryStackSize(targetItem, context);
            if (stackSize.MaximumStackSize == 1)
            {
                return TryReplaceItemInternal(context, default, targetItem, placementPos, forcePlacement, map, mapData);
            }

            var existingStack = context.ItemResolver.QueryStackSize(existingItem, context);
            if (!stackSize.Merge(existingStack, out var resultStack) ||
                !context.ItemResolver.TryUpdateData(targetItem, context, resultStack, out var changedItem))
            {
                // cannot merge items
                return false;
            }

            return TryReplaceItemInternal(context, existingItem, changedItem, placementPos, forcePlacement, map, mapData);
        }

        public bool TryReplaceItem(TGameContext context, in TItemId sourceItem, in TItemId targetItem, in Position p, bool forceMove = false)
        {
            if (p.IsInvalid)
            {
                logger.Verbose("Given position {position} is invalid", p);
                return false;
            }

            if (!mapLayerResolver(p.LayerId, out var mapLayer) ||
                !context.TryGetGridDataFor(mapLayer, out var mapData))
            {
                logger.Verbose("Unable to resolve grid data for map layer {p.LayerId} of position {position}", p.LayerId, p);
                return false;
            }

            if (!mapData.IsValid(p) ||
                !mapData.TryGetWritableView(p.GridZ, out var map, DataViewCreateMode.CreateMissing))
            {
                logger.Verbose("Requested grid position for map layer {p.LayerId} is out of range", p.LayerId, p);
                return false;
            }

            return TryReplaceItemInternal(context, sourceItem, targetItem, p, forceMove, map, mapData);
        }

        static bool TryReplaceItemInternal(TGameContext context,
                                           TItemId sourceItem,
                                           TItemId targetItem,
                                           Position p,
                                           bool forceMove,
                                           IView2D<TItemId> map,
                                           IGridMapDataContext<TItemId> mapData)
        {
            if (!Equality.Equals(sourceItem, map[p.GridX, p.GridY]))
            {
                logger.Verbose("Safety check failed: Unable to locate source item {sourceItem} at position {position}",
                               sourceItem, p);
                return false;
            }

            if (context.ItemResolver.TryUpdateData(sourceItem, context, EntityGridPosition.Invalid, out _))
            {
                // ensure that bulk items have been cleared out correctly before attempting 
                // to write the new data
                var old = map[p.GridX, p.GridY];
                map[p.GridX, p.GridY] = default;

                if (context.ItemResolver.TryUpdateData(targetItem, context, p, out _))
                {
                    mapData.MarkDirty(p);
                    return true;
                }

                // restore the old state if writing the new position data failed.
                map[p.GridX, p.GridY] = old;
            }

            if (Equality.Equals(targetItem, default))
            {
                mapData.MarkDirty(p);
                map[p.GridX, p.GridY] = default;
                return true;
            }

            if (!forceMove)
            {
                map[p.GridX, p.GridY] = sourceItem;
                return false;
            }

            if (!context.ItemResolver.TryQueryData(targetItem, context, out ImmobilityMarker _))
            {
                map[p.GridX, p.GridY] = targetItem;
                mapData.MarkDirty(p);
                return true;
            }

            return false;
        }
    }
}