using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Movement.ItemCosts
{
    public class StaticMovementCostPropertiesTrait<TGameContext, TItemId> : StatelessItemComponentTraitBase<TGameContext, TItemId, MovementCostProperties>
        where TItemId : IEntityKey
    {
        public StaticMovementCostPropertiesTrait(MovementCostProperties movementCosts) : base("Core.Item.MovementCost", 100)
        {
            this.InitialValue = movementCosts;
        }

        protected MovementCostProperties InitialValue { get; }

        protected override MovementCostProperties GetData(TGameContext context, TItemId k)
        {
            return InitialValue;
        }
    }
}