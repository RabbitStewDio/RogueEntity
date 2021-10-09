using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning.Grid
{
    public class ReferenceItemGridPositionTrait<TItemId> : IReferenceItemTrait<TItemId>,
                                                           IItemComponentTrait<TItemId, EntityGridPositionUpdateMessage>,
                                                           IItemComponentDesignTimeInformationTrait<MapLayerPreference>,
                                                           IItemComponentInformationTrait<TItemId, EntityGridPosition>,
                                                           IItemComponentInformationTrait<TItemId, Position>,
                                                           IItemComponentInformationTrait<TItemId, MapLayerPreference>,
                                                           IItemComponentInformationTrait<TItemId, MapContainerEntityMarker>
        where TItemId : IEntityKey
    {
        readonly MapLayerPreference layerPreference;

        public ReferenceItemGridPositionTrait(MapLayer layer,
                                              params MapLayer[] layers)
        {
            Id = "ReferenceItem.Generic.Position.Grid";
            Priority = 100;

            layerPreference = new MapLayerPreference(layer, layers);
        }

        public ItemTraitId Id { get; }
        public int Priority { get; }

        public IReferenceItemTrait<TItemId> CreateInstance()
        {
            return this;
        }

        public bool TryQuery(out MapLayerPreference t)
        {
            t = layerPreference;
            return true;
        }

        public void Initialize(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
        {
            v.AssignComponent(k, EntityGridPosition.Invalid);
        }

        public void Apply(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
        { }

        
        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out EntityGridPosition t)
        {
            if (v.IsValid(k) &&
                v.GetComponent(k, out t))
            {
                return true;
            }

            t = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out Position t)
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
        
        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out MapContainerEntityMarker t)
        {
            if (v.IsValid(k) && v.GetComponent(k, out EntityGridPosition _))
            {
                t = new MapContainerEntityMarker();
                return true;
            }

            t = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out EntityGridPositionUpdateMessage t)
        {
            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TItemId k, in EntityGridPositionUpdateMessage t, out TItemId changedK)
        {
            v.AssignOrReplace(k, t.Data);
            changedK = k;
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> v, TItemId k, out TItemId changedK)
        {
            changedK = k;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out MapLayerPreference t)
        {
            t = layerPreference;
            return true;
        }

        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return GridPositionModule.GridPositionedRole.Instantiate<TItemId>();
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }
    }
}
