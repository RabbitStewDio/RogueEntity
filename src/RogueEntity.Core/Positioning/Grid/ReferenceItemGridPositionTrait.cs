using System;
using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.Positioning.Grid
{
    public class ReferenceItemGridPositionTrait<TGameContext, TItemId> : IReferenceItemTrait<TGameContext, TItemId>,
                                                                         IItemComponentTrait<TGameContext, TItemId, Position>,
                                                                         IItemComponentTrait<TGameContext, TItemId, EntityGridPosition>,
                                                                         IItemComponentInformationTrait<TGameContext, TItemId, EntityGridPositionChangedMarker>,
                                                                         IItemComponentDesignTimeInformationTrait<MapLayerPreference>,
                                                                         IItemComponentInformationTrait<TGameContext, TItemId, MapLayerPreference>,
                                                                         IItemComponentInformationTrait<TGameContext, TItemId, MapContainerEntityMarker>
        where TItemId : IEntityKey
    {
        readonly IItemResolver<TGameContext, TItemId> itemResolver;
        readonly MapLayerPreference layerPreference;
        readonly ILogger logger = SLog.ForContext<ReferenceItemGridPositionTrait<TGameContext, TItemId>>();
        readonly IGridMapContext<TItemId> gridContext;

        public ReferenceItemGridPositionTrait([NotNull] IItemResolver<TGameContext, TItemId> itemResolver,
                                              [NotNull] IGridMapContext<TItemId> gridContext, 
                                              MapLayer layer, params MapLayer[] layers)
        {
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
            this.gridContext = gridContext ?? throw new ArgumentNullException(nameof(gridContext));
            Id = "ReferenceItem.Generic.Position.Grid";
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

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out MapContainerEntityMarker t)
        {
            if (v.IsValid(k) && v.GetComponent(k, out EntityGridPosition _))
            {
                t = new MapContainerEntityMarker();
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
                    !gridContext.TryGetGridDataFor(previousLayerId, out var previousMapContext) ||
                    !previousMapContext.TryGetWritableView(previousPosition.GridZ, out var previousMap))
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

            if (!gridContext.TryGetGridDataFor(layerId, out var mapDataContext) ||
                !mapDataContext.TryGetWritableView(desiredPosition.GridZ, out var targetMap, DataViewCreateMode.CreateMissing))
            {
                logger.Warning("Invalid layer {Layer} for unresolvable map data for item {ItemId}", layerId, k);
                changedK = k;
                return false;
            }

            var gridX = desiredPosition.GridX;
            var gridY = desiredPosition.GridY;
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
                    !gridContext.TryGetGridDataFor(previousLayerId, out var previousItemMap) ||
                    !previousItemMap.TryGetWritableView(previousPosition.GridZ, out var previousMap))
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