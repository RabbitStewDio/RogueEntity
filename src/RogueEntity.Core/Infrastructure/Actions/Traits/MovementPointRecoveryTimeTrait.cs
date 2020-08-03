using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Actions.Traits
{
    public class MovementPointRecoveryTimeTrait<TGameContext, TActorId> : SimpleItemComponentTraitBase<TGameContext, TActorId, MovementPointRecoveryTime>
        where TActorId : IBulkDataStorageKey<TActorId>
    {
        readonly ActionPointRecoveryDefinition recoveryDefinition;

        public MovementPointRecoveryTimeTrait(ActionPointRecoveryDefinition recoveryDefinition) : base("Core.Actor.MovementPointRecovery", 100)
        {
            this.recoveryDefinition = recoveryDefinition;
        }

        protected override MovementPointRecoveryTime CreateInitialValue(TGameContext c, TActorId actor)
        {
            return new MovementPointRecoveryTime(recoveryDefinition.Magnitude, recoveryDefinition.Frequency, 0);
        }
    }
}