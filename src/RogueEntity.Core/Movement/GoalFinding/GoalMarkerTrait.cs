using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Movement.GoalFinding
{
    public class GoalMarkerTrait<TGameContext, TItemId, TDiscriminator>: SimpleReferenceItemComponentTraitBase<TGameContext, TItemId, GoalMarker<TDiscriminator>>
        where TItemId : IEntityKey
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

        protected override GoalMarker<TDiscriminator> CreateInitialValue(TGameContext c, TItemId reference)
        {
            return new GoalMarker<TDiscriminator>(goalStrength);
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield break; // todo
        }
    }
}