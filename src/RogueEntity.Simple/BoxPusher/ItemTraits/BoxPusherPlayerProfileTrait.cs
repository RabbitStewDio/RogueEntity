using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;

namespace RogueEntity.Simple.BoxPusher.ItemTraits
{
    public class BoxPusherPlayerProfileTrait<TActorId>: SimpleReferenceItemComponentTraitBase<TActorId, BoxPusherPlayerProfile>
        where TActorId : IEntityKey
    {
        public BoxPusherPlayerProfileTrait() : base("BoxPusher.PlayerProfile-Trait", 100)
        {
        }

        protected override BoxPusherPlayerProfile CreateInitialValue(TActorId reference)
        {
            return new BoxPusherPlayerProfile();
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield break;
        }
    }
}
