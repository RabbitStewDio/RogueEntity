using EnTTSharp;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;

namespace RogueEntity.Samples.BoxPusher.Core.ItemTraits
{
    public class BoxPusherTargetFieldMarkerTrait<TItemId>: SimpleReferenceItemComponentTraitBase<TItemId, BoxPusherTargetFieldMarker>
        where TItemId : struct, IEntityKey
    {
        public BoxPusherTargetFieldMarkerTrait() : base("BoxPusher.TargetFieldMarker", 100)
        {
        }

        protected override Optional<BoxPusherTargetFieldMarker> CreateInitialValue(TItemId reference)
        {
            return new BoxPusherTargetFieldMarker();
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return BoxPusherModule.TargetSpotRole.Instantiate<TItemId>();
        }
    }
}
