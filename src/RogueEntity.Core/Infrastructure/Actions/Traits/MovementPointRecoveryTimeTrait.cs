using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Actions.Traits
{
    public class MovementPointRecoveryTimeTrait<TGameContext, TActorId> : SimpleReferenceItemComponentTraitBase<TGameContext, TActorId, MovementPointRecoveryTime>
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

        public override void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            throw new System.NotImplementedException();
        }
    }
}