using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Movement.ItemCosts
{
    public class MovementCostPropertiesTrait<TGameContext, TItemId> : SimpleItemComponentTraitBase<TGameContext, TItemId, MovementCostProperties> 
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        public MovementCostPropertiesTrait(MovementCostProperties movementCosts) : base("Core.Item.MovementCost", 100)
        {
            this.InitialValue = movementCosts;
        }

        protected MovementCostProperties InitialValue { get; }

        protected override MovementCostProperties CreateInitialValue(TGameContext c, TItemId reference)
        {
            return InitialValue;
        }
    }
}