using EnTTSharp;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Goals
{
    public class GoalMarkerTrait< TItemId, TDiscriminator>: SimpleReferenceItemComponentTraitBase< TItemId, GoalMarker<TDiscriminator>>
        where TItemId : struct, IEntityKey
    {
        readonly float goalStrength;

        public GoalMarkerTrait(GoalMarker<TDiscriminator> goal) : 
            base("Core.Traits.Movement.GoalMarker+" + typeof(TDiscriminator).Name, 100)
        {
            this.goalStrength = goal.Strength;
        }

        public GoalMarkerTrait(float goalStrength) : 
            base("Core.Traits.Movement.GoalMarker+" + typeof(TDiscriminator).Name, 100)
        {
            this.goalStrength = goalStrength;
        }

        protected override Optional<GoalMarker<TDiscriminator>> CreateInitialValue(TItemId reference)
        {
            return new GoalMarker<TDiscriminator>(goalStrength);
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return GoalModule.GoalMarkerRole.Instantiate<TItemId>();
        }
    }
}