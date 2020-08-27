using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Actions.Traits
{
    public class ScheduledActionPlanTrait<TGameContext, TActorId> : SimpleReferenceItemComponentTraitBase<TGameContext, TActorId, ScheduledActionPlan<TGameContext, TActorId>> 
        where TActorId : IEntityKey
    {
        public ScheduledActionPlanTrait() : base("Core.Actor.ScheduledActionPlan", 100)
        {
        }

        protected override ScheduledActionPlan<TGameContext, TActorId> CreateInitialValue(TGameContext c, TActorId actor)
        {
            return new ScheduledActionPlan<TGameContext, TActorId>();
        }
    }
}