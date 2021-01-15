using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Positioning
{
    public class ImmobilityMarkerTrait<TItemId> : StatelessItemComponentTraitBase<TItemId, ImmobilityMarker>
        where TItemId : IEntityKey
    {
        public ImmobilityMarkerTrait(): base("Item.Generic.Positional", 100)
        {
        }

        protected override ImmobilityMarker GetData(TItemId k)
        {
            return new ImmobilityMarker();
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            return Enumerable.Empty<EntityRoleInstance>();
        }
    }
}