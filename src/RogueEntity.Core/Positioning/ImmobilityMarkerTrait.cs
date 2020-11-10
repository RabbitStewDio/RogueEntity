using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Positioning
{
    public class ImmobilityMarkerTrait<TGameContext, TItemId> : StatelessItemComponentTraitBase<TGameContext, TItemId, ImmobilityMarker>
        where TItemId : IEntityKey
    {
        public ImmobilityMarkerTrait(): base("Item.Generic.Positional", 100)
        {
        }

        protected override ImmobilityMarker GetData(TGameContext context, TItemId k)
        {
            return new ImmobilityMarker();
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            return Enumerable.Empty<EntityRoleInstance>();
        }
    }
}