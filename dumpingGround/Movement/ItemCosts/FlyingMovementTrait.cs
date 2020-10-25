﻿using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Movement.ItemCosts
{
    public class FlyingMovementTrait<TGameContext, TActorId> : StatelessItemComponentTraitBase<TGameContext, TActorId, FlyingMovementData>,
                                                               IItemComponentTrait<TGameContext, TActorId, IMovementTrait<TGameContext, TActorId>>,
                                                               IMovementTrait<TGameContext, TActorId>
        where TActorId : IEntityKey
        where TGameContext : IItemContext<TGameContext, TActorId>, IMapMovementPropertiesContext
    {
        public int Cost { get; }

        public FlyingMovementTrait(int cost) : base("Actor.Generic.Movement.Flying", 100)
        {
            Cost = cost;
        }

        protected override FlyingMovementData GetData(TGameContext context, TActorId k)
        {
            return InitialValue;
        }

        protected FlyingMovementData InitialValue => new FlyingMovementData(new MovementCost(Cost));

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out IMovementTrait<TGameContext, TActorId> t)
        {
            t = this;
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, in IMovementTrait<TGameContext, TActorId> t, out TActorId changedK)
        {
            changedK = k;
            return false;
        }

        public bool CanEnterCell(TActorId k, TGameContext context, Position position, out MovementCost movementCost)
        {
            if (position.IsInvalid ||
                !context.ItemResolver.TryQueryData(k, context, out FlyingMovementData baseCost))
            {
                movementCost = MovementCost.Blocked;
                return false;
            }

            if (CalculateVariableCellCost(context, position, out var variableMovementCost))
            {
                movementCost = baseCost.Cost.Combine(variableMovementCost);
                return movementCost != MovementCost.Blocked;
            }

            movementCost = MovementCost.Blocked;
            return false;
        }

        public bool CalculateVariableCellCost(TGameContext context, Position position, out MovementCost movementCost)
        {
            if (context.TryQueryMovementProperties(position, out var groundProps))
            {
                movementCost = groundProps.Flying;
            }
            else
            {
                movementCost = MovementCost.Blocked;
            }

            return movementCost != MovementCost.Blocked;
        }

        public MovementCost BaseMovementCost => InitialValue.Cost;
    }
}