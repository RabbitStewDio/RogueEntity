using System;
using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Meta.Items;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Positioning.Grid
{
    public class BulkItemGridPositionTrait<TGameContext, TItemId> : IBulkItemTrait<TGameContext, TItemId>,
                                                                IItemComponentTrait<TGameContext, TItemId, EntityGridPosition>,
                                                                IItemComponentTrait<TGameContext, TItemId, MapLayerPreference>
        where TGameContext : IGridMapContext<TGameContext, TItemId>, IItemContext<TGameContext, TItemId>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        readonly ILogger logger = SLog.ForContext<BulkItemGridPositionTrait<TGameContext, TItemId>>();
        readonly MapLayerPreference layerPreference;

        public BulkItemGridPositionTrait(MapLayer layer, params MapLayer[] layers)
        {
            Id = "ReferenceItem.Generic.Positional";
            Priority = 100;

            layerPreference = new MapLayerPreference(layer, layers);
        }

        public string Id { get; }
        public int Priority { get; }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out MapLayerPreference t)
        {
            t = layerPreference;
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, 
                              in MapLayerPreference t, out TItemId changedK)
        {
            changedK = k;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out EntityGridPosition t)
        {
            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId targetItem, 
                              in EntityGridPosition p, out TItemId changedK)
        {
            if (targetItem.IsReference)
            {
                throw new ArgumentException("Unable to process reference item here.");
            }

            if (p == EntityGridPosition.Invalid)
            {
                // Indicate to the caller that this operation can proceed.
                // An item will be removed from the map. We don't know the old position,
                // so we'll have to trust the caller to perform proper sanity checks.
                changedK = targetItem;
                return true;
            }

            if (!layerPreference.IsAcceptable(p, out var layerId))
            {
                WarnNotAcceptableLayer(context, targetItem, p);
                changedK = targetItem;
                return false;
            }

            if (!context.TryGetGridDataFor(layerId, out var mapDataContext) ||
                !mapDataContext.TryGetMap(p.GridZ, out var map))
            {
                logger.Warning("Invalid layer {Layer} for unresolvabled map data for item {ItemId}", p.LayerId, targetItem);
                changedK = targetItem;
                return false;
            }

            var itemAtPos = map[p.GridX, p.GridY];
            if (itemAtPos.IsEmpty)
            {
                map[p.GridX, p.GridY] = targetItem;
                changedK = targetItem;
                mapDataContext.MarkDirty(p);
                return true;
            }

            if (!itemAtPos.IsSameBulkDataType(targetItem))
            {
                // cannot merge items of different type.
                changedK = targetItem;
                return false;
            }

            var stackSizeOnMap = context.QueryStackSize(itemAtPos);
            var stackSizeNew = context.QueryStackSize(targetItem);
            if (stackSizeOnMap.Merge(stackSizeNew, out var merged) &&
                context.ItemResolver.TryUpdateData(itemAtPos, context, in merged, out var changedRef))
            {
                map[p.GridX, p.GridY] = changedRef;
                changedK = changedRef;
                mapDataContext.MarkDirty(p);
                return true;
            }

            changedK = targetItem;
            return false;
        }

        void WarnNotAcceptableLayer(TGameContext context, TItemId targetItem, EntityGridPosition p)
        {
            if (context.ItemResolver.TryResolve(targetItem, out var itemDef))
            {
                logger.Warning("Invalid layer {Layer} for item {ItemId}", p.LayerId, itemDef.Id);
            }
            else
            {
                logger.Warning("Invalid layer {Layer} for unresolvable item {ItemId}", p.LayerId, targetItem);
            }
        }

        public TItemId Initialize(TGameContext context, IItemDeclaration item, TItemId reference)
        {
            return reference;
        }
    }
}