using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Players;
using System.Collections.Generic;

namespace RogueEntity.Samples.BoxPusher.Core.ItemTraits
{
    public class BoxPusherPlayerProfileTrait<TActorId>: SimpleReferenceItemComponentTraitBase<TActorId, BoxPusherPlayerProfile>
        where TActorId : IEntityKey
    {
        public BoxPusherPlayerProfileTrait() : base("BoxPusher.PlayerProfile-Trait", 100)
        {
        }

        protected override Optional<BoxPusherPlayerProfile> CreateInitialValue(TActorId reference)
        {
            return new BoxPusherPlayerProfile();
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return PlayerModule.PlayerRole.Instantiate<TActorId>();
        }
    }
}
