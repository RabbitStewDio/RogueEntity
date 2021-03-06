using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;

namespace RogueEntity.Core.Movement.GridMovement
{
    public class MovementIntentTrait<TItemId>: SimpleReferenceItemComponentTraitBase<TItemId, MovementIntent>
        where TItemId : IEntityKey
    {
        public MovementIntentTrait() : base("Trait.Core.Movement.MovementIntent", 100)
        {
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return GridMovementModule.MovementIntentRole.Instantiate<TItemId>();
        }
    }
}
