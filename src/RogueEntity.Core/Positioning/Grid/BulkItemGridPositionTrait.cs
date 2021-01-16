using System;
using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.Positioning.Grid
{
    public class BulkItemGridPositionTrait<TItemId> : IBulkItemTrait<TItemId>,
                                                      IItemComponentTrait<TItemId, EntityGridPosition>,
                                                      IItemComponentDesignTimeInformationTrait<MapLayerPreference>,
                                                      IItemComponentInformationTrait<TItemId, MapLayerPreference>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        readonly IBulkDataStorageMetaData<TItemId> itemIdMetaData;
        readonly IItemResolver<TItemId> itemResolver;
        readonly ILogger logger = SLog.ForContext<BulkItemGridPositionTrait<TItemId>>();
        readonly MapLayerPreference layerPreference;
        readonly IGridMapContext<TItemId> gridMapContext;

        public BulkItemGridPositionTrait([NotNull] IBulkDataStorageMetaData<TItemId> itemIdMetaData,
                                         [NotNull] IItemResolver<TItemId> itemResolver,
                                         [NotNull] IGridMapContext<TItemId> gridMapContext,
                                         MapLayer layer,
                                         params MapLayer[] layers)
        {
            this.itemIdMetaData = itemIdMetaData ?? throw new ArgumentNullException(nameof(itemIdMetaData));
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
            this.gridMapContext = gridMapContext ?? throw new ArgumentNullException(nameof(gridMapContext));
            Id = "ReferenceItem.Generic.Positional";
            Priority = 100;

            layerPreference = new MapLayerPreference(layer, layers);
        }

        public ItemTraitId Id { get; }
        public int Priority { get; }

        public IBulkItemTrait<TItemId> CreateInstance()
        {
            return this;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out MapLayerPreference t)
        {
            t = layerPreference;
            return true;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out EntityGridPosition t)
        {
            t = default;
            return true;
        }

        bool IItemComponentTrait<TItemId, EntityGridPosition>.TryRemove(IEntityViewControl<TItemId> entityRegistry, TItemId k, out TItemId changedItem)
        {
            return TryUpdate(entityRegistry, k, EntityGridPosition.Invalid, out changedItem);
        }

        public bool TryQuery(out MapLayerPreference t)
        {
            t = layerPreference;
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v,
                              TItemId targetItem,
                              in EntityGridPosition p,
                              out TItemId changedK)
        {
            if (targetItem.IsReference)
            {
                throw new ArgumentException("Unable to process reference item here.");
            }

            changedK = targetItem;
            if (p == EntityGridPosition.Invalid)
            {
                // Indicate to the caller that this operation can proceed.
                // An item will be removed from the map. We don't know the old position,
                // so we'll have to trust the caller to perform proper sanity checks.
                return true;
            }

            if (!layerPreference.IsAcceptable(p, out var layerId))
            {
                WarnNotAcceptableLayer(targetItem, p);
                return false;
            }

            if (!gridMapContext.TryGetGridDataFor(layerId, out var mapDataContext) ||
                !mapDataContext.TryGetWritableView(p.GridZ, out var map, DataViewCreateMode.CreateMissing))
            {
                logger.Warning("Invalid layer {Layer} for unresolvable map data for item {ItemId}", p.LayerId, targetItem);
                return false;
            }

            var itemAtPos = map[p.GridX, p.GridY];
            if (itemAtPos.IsEmpty)
            {
                map[p.GridX, p.GridY] = targetItem;
                mapDataContext.MarkDirty(p);
                return true;
            }

            if (itemAtPos.IsReference || !itemIdMetaData.IsSameBulkType(itemAtPos, targetItem))
            {
                // cannot merge items of different type.
                changedK = targetItem;
                return false;
            }

            var stackSizeOnMap = itemResolver.QueryStackSize(itemAtPos);
            var stackSizeNew = itemResolver.QueryStackSize(targetItem);
            if (stackSizeOnMap.Merge(stackSizeNew, out var mergedStack) &&
                itemResolver.TryUpdateData(itemAtPos, in mergedStack, out var changedRef))
            {
                map[p.GridX, p.GridY] = changedRef;
                changedK = changedRef;
                mapDataContext.MarkDirty(p);
                return true;
            }

            return false;
        }

        void WarnNotAcceptableLayer(TItemId targetItem, EntityGridPosition p)
        {
            if (itemResolver.TryResolve(targetItem, out var itemDef))
            {
                logger.Warning("Invalid layer {Layer} for item {ItemId}", p.LayerId, itemDef.Id);
            }
            else
            {
                logger.Warning("Invalid layer {Layer} for unresolvable item {ItemId}", p.LayerId, targetItem);
            }
        }

        public TItemId Initialize(IItemDeclaration item, TItemId reference)
        {
            return reference;
        }

        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return PositionModule.GridPositionedRole.Instantiate<TItemId>();
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }
    }
}
