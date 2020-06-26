using System;
using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Meta.Items;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Positioning.Continuous
{
    public class ReferenceItemContinuousPositionTrait<TGameContext, TItemId> : IReferenceItemTrait<TGameContext, TItemId>,
                                                                               IItemComponentTrait<TGameContext, TItemId, ContinuousMapPosition>,
                                                                               IItemComponentTrait<TGameContext, TItemId, ContinuousMapPositionChangedMarker>,
                                                                               IItemComponentTrait<TGameContext, TItemId, MapLayerPreference>
        where TItemId : IBulkDataStorageKey<TItemId>
        where TGameContext : IContinuousMapContext<TGameContext, TItemId>, IItemContext<TGameContext, TItemId>
    {
        readonly MapLayerPreference layerPreference;
        readonly ILogger logger = SLog.ForContext<ReferenceItemContinuousPositionTrait<TGameContext, TItemId>>();

        public ReferenceItemContinuousPositionTrait(MapLayer layer, params MapLayer[] layers)
        {
            Id = "ReferenceItem.Generic.Position.Continuous";
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

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out ContinuousMapPosition t)
        {
            if (v.IsValid(k) &&
                v.GetComponent(k, out t))
            {
                return true;
            }

            t = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out ContinuousMapPositionChangedMarker t)
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

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in ContinuousMapPositionChangedMarker t, out TItemId changedK)
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
                              in ContinuousMapPosition desiredPosition, out TItemId changedK)
        {
            changedK = k;

            if (!v.IsValid(k))
            {
                throw new ArgumentException();
            }

            if (!v.GetComponent(k, out ContinuousMapPosition previousPosition))
            {
                previousPosition = ContinuousMapPosition.Invalid;
            }

            if (previousPosition == desiredPosition)
            {
                logger.Verbose("No need to update item {ItemId}", k);
                return true;
            }

            if (desiredPosition == ContinuousMapPosition.Invalid)
            {
                // was on map before, now no longer on map.
                if (!layerPreference.IsAcceptable(previousPosition, out var previousLayerId) ||
                    !context.TryGetContinuousDataFor(previousLayerId, out var previousMapContext))
                {
                    throw new ArgumentException("A previously set position was not accepted as layer target.");
                }

                if (!previousMapContext.TryUpdateItemPosition(k, default))
                {
                    throw new ArgumentException($"Failed to update position of item {k}.");
                }

                v.RemoveComponent<ContinuousMapPosition>(k);
                previousMapContext.MarkDirty(previousPosition);
                logger.Verbose("Desired position is invalid, removed item {Item} from map", k);
                return true;
            }

            if (!layerPreference.IsAcceptable(desiredPosition, out var layerId))
            {
                WarnNotAcceptableLayer(context, k, desiredPosition);
                return false;
            }

            if (!context.TryGetContinuousDataFor(layerId, out var mapDataContext))
            {
                logger.Warning("Invalid layer {Layer} for unresolvabled map data for item {ItemId}", layerId, k);
                changedK = k;
                return false;
            }

            if (mapDataContext.TryGetItemAt(desiredPosition, out var itemAtDesiredPosition) &&
                !itemAtDesiredPosition.IsEmpty)
            {
                // target position is not empty. We would overwrite 
                // an existing actor.
                logger.Verbose("Desired position is not empty for item {Item} at {Pos}", k, desiredPosition);
                return false;
            }

            if (previousPosition != ContinuousMapPosition.Invalid)
            {
                if (!layerPreference.IsAcceptable(previousPosition, out var previousLayerId) ||
                    !context.TryGetContinuousDataFor(previousLayerId, out var previousItemMap))
                {
                    throw new ArgumentException("A previously set position was not accepted as layer target.");
                }

                previousItemMap.MarkDirty(previousPosition);
            }

            if (!mapDataContext.TryUpdateItemPosition(k, desiredPosition))
            {
                logger.Verbose("Unable to update position for item {Item} at {Pos}", k, desiredPosition);
                return false;
            }

            v.AssignOrReplace(k, in desiredPosition);
            mapDataContext.MarkDirty(desiredPosition);
            logger.Verbose("Placed item {Item} at {Pos}", k, desiredPosition);
            return true;
        }

        void WarnNotAcceptableLayer(TGameContext context, TItemId targetItem, ContinuousMapPosition p)
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