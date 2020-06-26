using System;
using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Meta.Items;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Positioning.Grid
{
    public class ReferenceItemGridPositionTrait<TGameContext, TItemId> : IReferenceItemTrait<TGameContext, TItemId>,
                                                                         IItemComponentTrait<TGameContext, TItemId, EntityGridPosition>,
                                                                         IItemComponentTrait<TGameContext, TItemId, EntityGridPositionChangedMarker>,
                                                                         IItemComponentTrait<TGameContext, TItemId, MapLayerPreference>
        where TItemId : IBulkDataStorageKey<TItemId>
        where TGameContext : IGridMapContext<TGameContext, TItemId>, IItemContext<TGameContext, TItemId>
    {
        readonly MapLayerPreference layerPreference;
        readonly ILogger logger = SLog.ForContext<ReferenceItemGridPositionTrait<TGameContext, TItemId>>();

        public ReferenceItemGridPositionTrait(MapLayer layer, params MapLayer[] layers)
        {
            Id = "ReferenceItem.Generic.Position.Grid";
            Priority = 100;

            layerPreference = new MapLayerPreference(layer, layers);
        }

        public string Id { get; }
        public int Priority { get; }

        public void Initialize(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
        }

        public void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out EntityGridPosition t)
        {
            if (v.IsValid(k) &&
                v.GetComponent(k, out t))
            {
                return true;
            }

            t = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out EntityGridPositionChangedMarker t)
        {
            if (v.IsValid(k) && v.GetComponent(k, out t))
            {
                return true;
            }

            t = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out MapLayerPreference t)
        {
            t = layerPreference;
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in EntityGridPositionChangedMarker t, out TItemId changedK)
        {
            changedK = k;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in MapLayerPreference t, out TItemId changedK)
        {
            changedK = k;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k,
                              in EntityGridPosition desiredPosition, out TItemId changedK)
        {
            changedK = k;

            if (!v.IsValid(k))
            {
                throw new ArgumentException();
            }

            if (!v.GetComponent(k, out EntityGridPosition previousPosition))
            {
                previousPosition = EntityGridPosition.Invalid;
            }

            if (previousPosition == desiredPosition)
            {
                logger.Verbose("No need to update item {ItemId}", k);
                return true;
            }

            if (desiredPosition == EntityGridPosition.Invalid)
            {
                // was on map before, now no longer on map.
                if (!layerPreference.IsAcceptable(previousPosition, out var previousLayerId) ||
                    !context.TryGetGridDataFor(previousLayerId, out var previousMapContext) ||
                    !previousMapContext.TryGetMap(previousPosition.GridZ, out var previousMap))
                {
                    throw new ArgumentException("A previously set position was not accepted as layer target.");
                }

                previousMap[previousPosition.GridX, previousPosition.GridY] = default;
                v.RemoveComponent<EntityGridPosition>(k);
                previousMapContext.MarkDirty(previousPosition);
                logger.Verbose("Desired position is invalid, removed item {Item} from map", k);
                return true;
            }

            if (!layerPreference.IsAcceptable(desiredPosition, out var layerId))
            {
                WarnNotAcceptableLayer(context, k, desiredPosition);
                return false;
            }

            if (!context.TryGetGridDataFor(layerId, out var mapDataContext) ||
                !mapDataContext.TryGetMap(desiredPosition.GridZ, out var targetMap))
            {
                logger.Warning("Invalid layer {Layer} for unresolvabled map data for item {ItemId}", layerId, k);
                changedK = k;
                return false;
            }

            if (!targetMap[desiredPosition.GridX, desiredPosition.GridY].IsEmpty)
            {
                // target position is not empty. We would overwrite 
                // an existing actor.
                logger.Verbose("Desired position is not empty for item {Item} at {Pos}", k, desiredPosition);
                return false;
            }

            if (previousPosition != EntityGridPosition.Invalid)
            {
                if (!layerPreference.IsAcceptable(previousPosition, out var previousLayerId) ||
                    !context.TryGetGridDataFor(previousLayerId, out var previousItemMap) ||
                    !previousItemMap.TryGetMap(previousPosition.GridZ, out var previousMap))
                {
                    throw new ArgumentException("A previously set position was not accepted as layer target.");
                }

                previousMap[previousPosition.GridX, previousPosition.GridY] = default;
                previousItemMap.MarkDirty(previousPosition);
            }

            targetMap[desiredPosition.GridX, desiredPosition.GridY] = k;
            v.AssignOrReplace(k, in desiredPosition);
            mapDataContext.MarkDirty(desiredPosition);
            logger.Verbose("Placed item {Item} at {Pos}", k, desiredPosition);
            return true;
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
    }
}