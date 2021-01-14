using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
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
        public static BulkItemDeclarationBuilder<TGameContext, TItemId> 
            WithMovementCostModifier<TGameContext, TItemId, TMovementMode>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                           RelativeMovementCostModifier<TMovementMode> m)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return builder.WithTrait(new RelativeMovementCostModifierTrait<TGameContext, TItemId, TMovementMode>(m));
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> 
            WithMovementCostModifier<TGameContext, TItemId, TMovementMode>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                           RelativeMovementCostModifier<TMovementMode> m)
            where TItemId : IEntityKey
        {
            return builder.WithTrait(new RelativeMovementCostModifierTrait<TGameContext, TItemId, TMovementMode>(m));
        }
    }
}
