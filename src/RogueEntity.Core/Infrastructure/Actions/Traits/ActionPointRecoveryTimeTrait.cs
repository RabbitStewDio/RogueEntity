using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Actions.Traits
{
    public class ActionPointRecoveryTimeTrait<TGameContext, TActorId>: SimpleItemComponentTraitBase<TGameContext, TActorId, ActionPointRecoveryTime> 
        where TActorId : IBulkDataStorageKey<TActorId>
    {
        readonly ActionPointRecoveryDefinition recoveryDefinition;

        public ActionPointRecoveryTimeTrait(ActionPointRecoveryDefinition recoveryDefinition) : base("Core.Actor.ActionPointRecovery", 100)
        {
            this.recoveryDefinition = recoveryDefinition;
        }

        protected override ActionPointRecoveryTime CreateInitialValue(TGameContext c, TActorId actor)
        {
            return new ActionPointRecoveryTime(recoveryDefinition.Magnitude, recoveryDefinition.Frequency, 0);
        }
    }
}