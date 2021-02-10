using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning.Grid
{
    public class BulkItemGridPositionTrait<TItemId> : IBulkItemTrait<TItemId>,
                                                      IItemComponentTrait<TItemId, EntityGridPositionUpdateMessage>,
                                                      IItemComponentDesignTimeInformationTrait<MapLayerPreference>,
                                                      IItemComponentInformationTrait<TItemId, MapLayerPreference>
        where TItemId : IEntityKey
    {
        readonly MapLayerPreference layerPreference;

        public BulkItemGridPositionTrait(MapLayer layer,
                                         params MapLayer[] layers)
        {
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

        public bool TryQuery(out MapLayerPreference t)
        {
            t = layerPreference;
            return true;
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

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out EntityGridPositionUpdateMessage t)
        {
            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TItemId k, in EntityGridPositionUpdateMessage t, out TItemId changedK)
        {
            // just signals that everything is ok. 
            changedK = k;
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> v, TItemId k, out TItemId changedK)
        {
            changedK = k;
            return false;
        }
    }
}
