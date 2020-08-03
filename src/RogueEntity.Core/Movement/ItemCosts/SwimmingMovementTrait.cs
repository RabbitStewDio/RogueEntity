using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Meta.Items;
using RogueEntity.Core.Infrastructure.Positioning;

namespace RogueEntity.Core.Movement.ItemCosts
{
    public class SwimmingMovementTrait<TGameContext, TActorId> : StatelessItemComponentTraitBase<TGameContext, TActorId, SwimmingMovementData>,
                                                                 IItemComponentTrait<TGameContext, TActorId, IMovementTrait<TGameContext, TActorId>>,
                                                                 IMovementTrait<TGameContext, TActorId>
        where TActorId : IBulkDataStorageKey<TActorId>
        where TGameContext: IItemContext<TGameContext, TActorId>, IMapMovementPropertiesContext
    {
        public int Cost { get; }

        public SwimmingMovementTrait(int cost) : base("Actor.Generic.Movement.Swimming", 100)
        {
            Cost = cost;
        }

        protected override SwimmingMovementData GetData(TGameContext context, TActorId k)
        {
            return InitialValue;
        }

        protected SwimmingMovementData InitialValue => new SwimmingMovementData(new MovementCost(Cost));

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
            if (position.Invalid ||
                !context.ItemResolver.TryQueryData(k, context, out SwimmingMovementData baseCost))
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
                movementCost = groundProps.Swimming;
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