using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Movement.CostModifier
{
    public static class MovementCostModifiers
    {
        public static RelativeMovementCostModifier<TMovement> Blocked<TMovement>() => RelativeMovementCostModifier<TMovement>.Blocked;
        public static RelativeMovementCostModifier<TMovement> Clear<TMovement>() => RelativeMovementCostModifier<TMovement>.Unchanged;
        public static RelativeMovementCostModifier<TMovement> For<TMovement>(float costFactor) => new RelativeMovementCostModifier<TMovement>(costFactor);

    }

    public static class RelativeMovementCostModifierItemDeclarations
    {
        public static BulkItemDeclarationBuilder< TItemId> 
            WithMovementCostModifier< TItemId, TMovementMode>(this BulkItemDeclarationBuilder< TItemId> builder,
                                                                           RelativeMovementCostModifier<TMovementMode> m)
            where TItemId : struct, IEntityKey
        {
            return builder.WithTrait(new RelativeMovementCostModifierTrait< TItemId, TMovementMode>(m));
        }

        public static ReferenceItemDeclarationBuilder< TItemId> 
            WithMovementCostModifier< TItemId, TMovementMode>(this ReferenceItemDeclarationBuilder< TItemId> builder,
                                                                           RelativeMovementCostModifier<TMovementMode> m)
            where TItemId : struct, IEntityKey
        {
            return builder.WithTrait(new RelativeMovementCostModifierTrait< TItemId, TMovementMode>(m));
        }
    }
}
