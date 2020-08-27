using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Movement.ItemCosts
{
    public class CombinedMovementTrait<TGameContext, TActorId> : IMovementTrait<TGameContext, TActorId>,
                                                                 IItemComponentTrait<TGameContext, TActorId, IMovementTrait<TGameContext, TActorId>>
        where TActorId : IEntityKey
    {
        bool initialized;
        List<IMovementTrait<TGameContext, TActorId>> movementMethods;
        List<IMovementTrait<TGameContext, TActorId>> movementMethodRaw;

        public CombinedMovementTrait()
        {
            Id = "Actor.Generic.Movement";
            Priority = 0;
        }

        public string Id { get; }
        public int Priority { get; }

        public void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            if (!initialized)
            {
                initialized = true;
                Initialize(item);
            }
        }

        public void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
        }

        void Initialize(IItemDeclaration actor)
        {
            movementMethodRaw = actor.QueryAll(movementMethodRaw);
            if (movementMethods == null)
            {
                movementMethods = new List<IMovementTrait<TGameContext, TActorId>>();
            }
            else
            {
                movementMethods.Clear();
            }

            foreach (var m in movementMethodRaw)
            {
                if (m is CombinedMovementTrait<TGameContext, TActorId>)
                {
                    continue;
                }

                movementMethods.Add(m);
            }
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out IMovementTrait<TGameContext, TActorId> t)
        {
            if (movementMethods.Count == 1)
            {
                t = movementMethods[0];
                return true;
            }

            t = this;
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, in IMovementTrait<TGameContext, TActorId> t, out TActorId changedK)
        {
            changedK = k;
            return false;
        }

        public bool TryRemove(IEntityViewControl<TActorId> entityRegistry, TGameContext context, TActorId k, out TActorId changedItem)
        {
            changedItem = k;
            return false;
        }

        public bool CanEnterCell(TActorId k, TGameContext context, Position position, out MovementCost movementCost)
        {
            var mc = MovementCost.Blocked;
            foreach (var m in movementMethods)
            {
                if (m.CanEnterCell(k, context, position, out var mcc))
                {
                    mc = mc.Reduce(mcc);
                }
            }

            movementCost = mc;
            return movementCost != MovementCost.Blocked;
        }

        public MovementCost BaseMovementCost
        {
            get
            {
                var mc = MovementCost.Blocked;
                foreach (var m in movementMethods)
                {
                    mc = mc.Reduce(m.BaseMovementCost);
                }

                return mc;
            }
        }

        public bool CalculateVariableCellCost(TGameContext context, Position position, out MovementCost movementCost)
        {
            var mc = MovementCost.Blocked;
            foreach (var m in movementMethods)
            {
                if (m.CalculateVariableCellCost(context, position, out var cost))
                {
                    mc = mc.Reduce(cost);
                }
            }

            movementCost = mc;
            return mc != MovementCost.Blocked;
        }
    }
}