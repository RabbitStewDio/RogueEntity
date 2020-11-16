using System;
using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Positioning.Continuous
{
    public class ReferenceItemContinuousPositionTrait<TGameContext, TItemId> : IReferenceItemTrait<TGameContext, TItemId>,
                                                                               IItemComponentTrait<TGameContext, TItemId, Position>,
                                                                               IItemComponentTrait<TGameContext, TItemId, ContinuousMapPosition>,
                                                                               IItemComponentInformationTrait<TGameContext, TItemId, ContinuousMapPositionChangedMarker>,
                                                                               IItemComponentInformationTrait<TGameContext, TItemId, MapLayerPreference>,
                                                                               IItemComponentDesignTimeInformationTrait<MapLayerPreference>,
                                                                               IItemComponentInformationTrait<TGameContext, TItemId, MapContainerEntityMarker>
        where TItemId : IEntityKey
        where TGameContext : IContinuousMapContext<TGameContext, TItemId>
    {
        readonly IItemResolver<TGameContext, TItemId> itemResolver;
        readonly MapLayerPreference layerPreference;
        readonly ILogger logger = SLog.ForContext<ReferenceItemContinuousPositionTrait<TGameContext, TItemId>>();

        public ReferenceItemContinuousPositionTrait(IItemResolver<TGameContext, TItemId> itemResolver, 
                                                    MapLayer layer, params MapLayer[] layers)
        {
            this.itemResolver = itemResolver;
            Id = "ReferenceItem.Generic.Position.Continuous";
            Priority = 100;

            layerPreference = new MapLayerPreference(layer, layers);
        }

        public ItemTraitId Id { get; }
        public int Priority { get; }

        public IReferenceItemTrait<TGameContext, TItemId> CreateInstance()
        {
            return this;
        }

        public bool TryQuery(out MapLayerPreference t)
        {
            t = layerPreference;
            return true;
        }

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

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out Position t)
        {
            if (v.IsValid(k) &&
                v.GetComponent(k, out ContinuousMapPosition p))
            {
                t = Position.From(p);
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

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out MapContainerEntityMarker t)
        {
            if (v.IsValid(k) && v.GetComponent(k, out ContinuousMapPosition _))
            {
                t = new MapContainerEntityMarker();
                return true;
            }

            t = default;
            return false;
        }

        bool IItemComponentTrait<TGameContext, TItemId, ContinuousMapPosition>.TryRemove(IEntityViewControl<TItemId> entityRegistry, TGameContext context, TItemId k, out TItemId changedItem)
        {
            return TryUpdate(entityRegistry, context, k, ContinuousMapPosition.Invalid, out changedItem);
        }

        bool IItemComponentTrait<TGameContext, TItemId, Position>.TryRemove(IEntityViewControl<TItemId> entityRegistry, TGameContext context, TItemId k, out TItemId changedItem)
        {
            return TryUpdate(entityRegistry, context, k, ContinuousMapPosition.Invalid, out changedItem);
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in Position t, out TItemId changedK)
        {
            return TryUpdate(v, context, k, ContinuousMapPosition.From(t), out changedK);
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
                WarnNotAcceptableLayer(k, desiredPosition);
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

        void WarnNotAcceptableLayer(TItemId targetItem, ContinuousMapPosition p)
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

        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return PositionModule.ContinuousPositionedRole.Instantiate<TItemId>();
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }
        
    }
}