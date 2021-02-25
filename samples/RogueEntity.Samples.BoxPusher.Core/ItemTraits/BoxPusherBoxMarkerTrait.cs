using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;

namespace RogueEntity.Samples.BoxPusher.Core.ItemTraits
{
    public class BoxPusherBoxMarkerTrait<TItemId>: SimpleReferenceItemComponentTraitBase<TItemId, BoxPusherBoxMarker>
        where TItemId : IEntityKey
    {
        public BoxPusherBoxMarkerTrait() : base("BoxPusher.BoxMarker", 100)
        {
        }

        protected override Optional<BoxPusherBoxMarker> CreateInitialValue(TItemId reference)
        {
            return new BoxPusherBoxMarker();
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return BoxPusherModule.BoxRole.Instantiate<TItemId>();
        }
    }
}
