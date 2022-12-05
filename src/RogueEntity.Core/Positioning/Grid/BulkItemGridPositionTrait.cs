using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning.Grid
{
    public class BulkItemGridPositionTrait<TItemId> : IBulkItemTrait<TItemId>,
                                                      IItemComponentTrait<TItemId, Position>,
                                                      IItemComponentTrait<TItemId, EntityGridPosition>,
                                                      IItemComponentDesignTimeInformationTrait<MapLayerPreference>,
                                                      IItemComponentInformationTrait<TItemId, MapLayerPreference>,
                                                      IItemComponentInformationTrait<TItemId, BodySize>
        where TItemId : struct, IEntityKey
    {
        readonly BodySize bodySize;
        readonly MapLayerPreference layerPreference;

        public BulkItemGridPositionTrait(BodySize bodySize,
                                         MapLayer layer,
                                         params MapLayer[] layers)
        {
            this.bodySize = bodySize;
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

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out BodySize t)
        {
            t = bodySize;
            return true;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out MapLayerPreference t)
        {
            t = layerPreference;
            return true;
        }

        public bool TryQuery(out MapLayerPreference t)
        {
            t = layerPreference;
            return true;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out Position t)
        {
            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TItemId k, in Position t, out TItemId changedK)
        {
            changedK = k;
            return true;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out EntityGridPosition t)
        {
            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TItemId k, in EntityGridPosition t, out TItemId changedK)
        {
            changedK = k;
            return true;
        }

        public TItemId Initialize(IItemDeclaration item, TItemId reference)
        {
            return reference;
        }

        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return GridPositionModule.GridPositionedRole.Instantiate<TItemId>();
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }

        public bool TryRemove(IEntityViewControl<TItemId> v, TItemId k, out TItemId changedK)
        {
            changedK = k;
            return false;
        }
    }
}