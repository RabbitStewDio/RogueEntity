using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Infrastructure.Actions.Schedule;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Actions.Traits
{
    public class ScheduledActionPlanTrait< TActorId> : SimpleReferenceItemComponentTraitBase< TActorId, ScheduledActionPlan< TActorId>> 
        where TActorId : IEntityKey
    {
        public ScheduledActionPlanTrait() : base("Core.Actor.ScheduledActionPlan", 100)
        {
        }

        protected override ScheduledActionPlan< TActorId> CreateInitialValue(TActorId actor)
        {
            return new ScheduledActionPlan< TActorId>();
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            throw new System.NotImplementedException();
        }
    }
}