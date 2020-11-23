using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Movement.CostModifier
{
    public static class RelativeMovementCostModifierItemDeclarations
    {
        public static MovementCostModifierBulkItemBuilder<TGameContext, TItemId> WithMovementCostModifier<TGameContext, TItemId>(
            this BulkItemDeclarationBuilder<TGameContext, TItemId> builder)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return new MovementCostModifierBulkItemBuilder<TGameContext, TItemId>(builder);
        }

        public static MovementCostModifierReferenceItemBuilder<TGameContext, TItemId> WithMovementCostModifier<TGameContext, TItemId>(
            this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder)
            where TItemId : IEntityKey
        {
            return new MovementCostModifierReferenceItemBuilder<TGameContext, TItemId>(builder);
        }

        public readonly struct MovementCostModifierReferenceItemBuilder<TGameContext, TItemId>
            where TItemId : IEntityKey
        {
            readonly ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder;

            public MovementCostModifierReferenceItemBuilder(ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder)
            {
                this.builder = builder;
            }

            public ReferenceItemDeclarationBuilder<TGameContext, TItemId> For<TMovementMode>(float relativeCostFactor)
                where TMovementMode : IMovementMode
            {
                return this.builder.WithTrait(new RelativeMovementCostModifierTrait<TGameContext, TItemId, TMovementMode>(relativeCostFactor));
            }

            public ReferenceItemDeclarationBuilder<TGameContext, TItemId> Block<TMovementMode>()
                where TMovementMode : IMovementMode
            {
                return this.builder.WithTrait(new RelativeMovementCostModifierTrait<TGameContext, TItemId, TMovementMode>(RelativeMovementCostModifier<TMovementMode>.Blocked));
            }

            public ReferenceItemDeclarationBuilder<TGameContext, TItemId> Clear<TMovementMode>()
                where TMovementMode : IMovementMode
            {
                return this.builder.WithTrait(new RelativeMovementCostModifierTrait<TGameContext, TItemId, TMovementMode>(RelativeMovementCostModifier<TMovementMode>.Unchanged));
            }
        }

        public readonly struct MovementCostModifierBulkItemBuilder<TGameContext, TItemId>
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            readonly BulkItemDeclarationBuilder<TGameContext, TItemId> builder;

            public MovementCostModifierBulkItemBuilder(BulkItemDeclarationBuilder<TGameContext, TItemId> builder)
            {
                this.builder = builder;
            }

            public BulkItemDeclarationBuilder<TGameContext, TItemId> For<TMovementMode>(float relativeCostFactor)
                where TMovementMode : IMovementMode
            {
                return this.builder.WithTrait(new RelativeMovementCostModifierTrait<TGameContext, TItemId, TMovementMode>(relativeCostFactor));
            }

            public BulkItemDeclarationBuilder<TGameContext, TItemId> Block<TMovementMode>()
                where TMovementMode : IMovementMode
            {
                return this.builder.WithTrait(new RelativeMovementCostModifierTrait<TGameContext, TItemId, TMovementMode>(RelativeMovementCostModifier<TMovementMode>.Blocked));
            }

            public BulkItemDeclarationBuilder<TGameContext, TItemId> Clear<TMovementMode>()
                where TMovementMode : IMovementMode
            {
                return this.builder.WithTrait(new RelativeMovementCostModifierTrait<TGameContext, TItemId, TMovementMode>(RelativeMovementCostModifier<TMovementMode>.Unchanged));
            }
        }
    }
}