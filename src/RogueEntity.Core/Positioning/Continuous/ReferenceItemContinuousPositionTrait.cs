using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning.Continuous
{
    public class ReferenceItemContinuousPositionTrait<TItemId> : IReferenceItemTrait<TItemId>,
                                                                 IItemComponentDesignTimeInformationTrait<MapLayerPreference>,
                                                                 IItemComponentInformationTrait<TItemId, ContinuousMapPosition>,
                                                                 IItemComponentInformationTrait<TItemId, Position>,
                                                                 IItemComponentInformationTrait<TItemId, MapLayerPreference>,
                                                                 IItemComponentInformationTrait<TItemId, MapContainerEntityMarker>,
                                                                 IItemComponentInformationTrait<TItemId, BodySize>
        where TItemId : struct, IEntityKey
    {
        readonly BodySize bodySize;
        readonly MapLayerPreference layerPreference;

        public ReferenceItemContinuousPositionTrait(BodySize bodySize,
                                                    MapLayer layer,
                                                    params MapLayer[] layers)
        {
            this.bodySize = bodySize;
            Id = "ReferenceItem.Generic.Position.Continuous";
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
        { }

        public void Apply(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
        { }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out ContinuousMapPosition t)
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
                v.GetComponent(k, out ContinuousMapPosition p))
            {
                t = Position.From(p);
                return true;
            }

            t = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out MapPositionChangedMarker t)
        {
            if (v.IsValid(k) && v.GetComponent(k, out t))
            {
                return true;
            }

            t = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out MapLayerPreference t)
        {
            t = layerPreference;
            return true;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out MapContainerEntityMarker t)
        {
            if (v.IsValid(k) && v.GetComponent(k, out ContinuousMapPosition _))
            {
                t = new MapContainerEntityMarker();
                return true;
            }

            t = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out BodySize t)
        {
            t = bodySize;
            return true;
        }

        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return ContinuousPositionModule.ContinuousPositionedRole.Instantiate<TItemId>();
            yield return PositionModule.PositionQueryRole.Instantiate<TItemId>();
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }
    }
}
