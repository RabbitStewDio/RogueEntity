using System;
using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Positioning.Grid
{
    public class ReferenceItemGridPositionTrait<TGameContext, TItemId> : IReferenceItemTrait<TGameContext, TItemId>,
                                                                         IItemComponentTrait<TGameContext, TItemId, Position>,
                                                                         IItemComponentTrait<TGameContext, TItemId, EntityGridPosition>,
                                                                         IItemComponentTrait<TGameContext, TItemId, EntityGridPositionChangedMarker>,
                                                                         IItemComponentTrait<TGameContext, TItemId, MapLayerPreference>
        where TItemId : IBulkDataStorageKey<TItemId>
        where TGameContext : IGridMapContext<TGameContext, TItemId>
    {
        readonly IItemResolver<TGameContext, TItemId> itemResolver;
        readonly MapLayerPreference layerPreference;
        readonly ILogger logger = SLog.ForContext<ReferenceItemGridPositionTrait<TGameContext, TItemId>>();

        public ReferenceItemGridPositionTrait(IItemResolver<TGameContext, TItemId> itemResolver, 
                                              MapLayer layer, params MapLayer[] layers)
        {
            this.itemResolver = itemResolver;
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

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out Position t)
        {
            if (v.IsValid(k) &&
                v.GetComponent(k, out EntityGridPosition p))
            {
                t = Position.From(p);
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

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in Position t, out TItemId changedK)
        {
            return TryUpdate(v, context, k, EntityGridPosition.From(t), out changedK);
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
                logger.Verbose("Desired position is EntityGridPosition.Invalid, therefore removed item {Item} from map", k);
                return true;
            }

            if (!layerPreference.IsAcceptable(desiredPosition, out var layerId))
            {
                WarnNotAcceptableLayer(k, desiredPosition);
                return false;
            }

            if (!context.TryGetGridDataFor(layerId, out var mapDataContext) ||
                !mapDataContext.TryGetMap(desiredPosition.GridZ, out var targetMap))
            {
                logger.Warning("Invalid layer {Layer} for unresolvable map data for item {ItemId}", layerId, k);
                changedK = k;
                return false;
            }

            var gridX = desiredPosition.GridX;
            var gridY = desiredPosition.GridY;
            if (gridX < 0 || gridY < 0 || gridX >= targetMap.Width || gridY >= targetMap.Height)
            {
                logger.Verbose("Desired position is out of range for map size of {Size} with {Pos}", (gridX, gridY), desiredPosition);
                return false;
            }
            if (!targetMap[gridX, gridY].IsEmpty)
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

            targetMap[gridX, gridY] = k;
            v.AssignOrReplace(k, in desiredPosition);
            mapDataContext.MarkDirty(desiredPosition);
            logger.Verbose("Placed item {Item} at {Pos}", k, desiredPosition);
            return true;
        }

        bool IItemComponentTrait<TGameContext, TItemId, EntityGridPosition>.TryRemove(IEntityViewControl<TItemId> entityRegistry, TGameContext context, TItemId k, out TItemId changedItem)
        {
            return TryUpdate(entityRegistry, context, k, EntityGridPosition.Invalid, out changedItem);
        }

        public bool TryRemove(IEntityViewControl<TItemId> entityRegistry, TGameContext context, TItemId k, out TItemId changedItem)
        {
            return TryUpdate(entityRegistry, context, k, EntityGridPosition.Invalid, out changedItem);
        }

        bool IItemComponentTrait<TGameContext, TItemId, EntityGridPositionChangedMarker>.TryRemove(IEntityViewControl<TItemId> entityRegistry, TGameContext context, TItemId k, out TItemId changedItem)
        {
            return TryUpdate(entityRegistry, context, k, EntityGridPosition.Invalid, out changedItem);
        }

        bool IItemComponentTrait<TGameContext, TItemId, MapLayerPreference>.TryRemove(IEntityViewControl<TItemId> entityRegistry, TGameContext context, TItemId k, out TItemId changedItem)
        {
            changedItem = k;
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
    }
}