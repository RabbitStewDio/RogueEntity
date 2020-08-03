﻿using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Actions.Traits
{
    public class MovementPointsTrait<TGameContext, TActorId> : SimpleReferenceItemComponentTraitBase<TGameContext, TActorId, MovementPoints> 
        where TActorId : IEntityKey
    {
        readonly int initialValue;

        public MovementPointsTrait(int initialValue = 0) : base("Core.Actor.MovementPoints", 100)
        {
            this.initialValue = initialValue;
        }

        protected override MovementPoints CreateInitialValue(TGameContext c, TActorId reference)
        {
            return MovementPoints.From(initialValue);
        }
    }
}