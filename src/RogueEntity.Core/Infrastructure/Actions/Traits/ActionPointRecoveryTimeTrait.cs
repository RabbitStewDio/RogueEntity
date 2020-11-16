﻿using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

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

        public override void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            throw new System.NotImplementedException();
        }
    }
}