using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Core.Players
{
    public class PlayerSpawnLocationTrait<TItemId> : SimpleReferenceItemComponentTraitBase<TItemId, PlayerSpawnLocation>
        where TItemId : IEntityKey
    {
        public PlayerSpawnLocationTrait() : base("Core.Players.SpawnLocation", 100)
        {
        }

        protected override PlayerSpawnLocation CreateInitialValue(TItemId reference)
        {
            return new PlayerSpawnLocation();
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            return Enumerable.Empty<EntityRoleInstance>();
        }
    }
}