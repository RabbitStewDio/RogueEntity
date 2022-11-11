using EnTTSharp;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Goals
{
    public interface IGoalMarkerTraitInformation
    {
        public Type GoalType { get; }
    }
    
    public class GoalMarkerTrait< TItemId, TGoal>: SimpleReferenceItemComponentTraitBase< TItemId, GoalMarker<TGoal>>, IGoalMarkerTraitInformation
        where TItemId : struct, IEntityKey
        where TGoal: IGoal
    {
        readonly float goalStrength;

        public GoalMarkerTrait(GoalMarker<TGoal> goal) : 
            base("Core.Traits.Movement.GoalMarker+" + typeof(TGoal).Name, 100)
        {
            this.goalStrength = goal.Strength;
        }

        public GoalMarkerTrait(float goalStrength) : 
            base("Core.Traits.Movement.GoalMarker+" + typeof(TGoal).Name, 100)
        {
            this.goalStrength = goalStrength;
        }

        public Type GoalType => typeof(TGoal);

        protected override Optional<GoalMarker<TGoal>> CreateInitialValue(TItemId reference)
        {
            return new GoalMarker<TGoal>(goalStrength);
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return GoalModule.GoalMarkerRole.Instantiate<TItemId>();
            yield return GoalModule.GetGoalMarkerInstanceRole<TGoal>().Instantiate<TItemId>();
        }
    }
}