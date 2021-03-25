using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;

namespace RogueEntity.Core.Players
{
    public class PlayerSpawnLocationTrait<TItemId> : SimpleReferenceItemComponentTraitBase<TItemId, PlayerSpawnLocation>
        where TItemId : IEntityKey
    {
        public PlayerSpawnLocationTrait() : base("Core.Players.SpawnLocation", 100)
        {
        }

        protected override Optional<PlayerSpawnLocation> CreateInitialValue(TItemId reference)
        {
            return new PlayerSpawnLocation();
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return PlayerModule.PlayerSpawnPointRole.Instantiate<TItemId>();
        }
    }
}
