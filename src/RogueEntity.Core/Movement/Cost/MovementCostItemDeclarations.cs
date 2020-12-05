using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Movement.Cost
{
    public static class MovementCostItemDeclarations
    {
        public static MovementCostBulkItemBuilder<TGameContext, TItemId> WithMovement<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return new MovementCostBulkItemBuilder<TGameContext, TItemId> (builder);
        }

        public static MovementCostReferenceItemBuilder<TGameContext, TItemId> WithMovement<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder)
            where TItemId : IEntityKey
        {
            return new MovementCostReferenceItemBuilder<TGameContext, TItemId>(builder);
        }

        public readonly struct MovementCostReferenceItemBuilder<TGameContext, TItemId>
            where TItemId : IEntityKey
        {
            readonly ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder;

            public MovementCostReferenceItemBuilder(ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder)
            {
                this.builder = builder;
            }

            public ReferenceItemDeclarationBuilder<TGameContext, TItemId> AsPointCost<TMovementMode>(TMovementMode m, DistanceCalculation c, float unitCost, int preference = 0)
                where TMovementMode : IMovementMode
            {
                this.builder.WithTrait(new MovementPointCostReferenceItemTrait<TGameContext, TItemId, TMovementMode>(m, c, unitCost, preference));
                this.builder.WithTrait(new PathfindingMovementCostFactorsTrait<TGameContext, TItemId>());
                return builder;
            }
            public ReferenceItemDeclarationBuilder<TGameContext, TItemId> AsVelocity<TMovementMode>(TMovementMode m, DistanceCalculation c, float unitCost, int preference = 0)
                where TMovementMode : IMovementMode
            {
                this.builder.WithTrait(new MovementVelocityReferenceItemTrait<TGameContext, TItemId, TMovementMode>(m, c, unitCost, preference));
                this.builder.WithTrait(new PathfindingMovementCostFactorsTrait<TGameContext, TItemId>());
                return builder;
            }
        }

        public readonly struct MovementCostBulkItemBuilder<TGameContext, TItemId>
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            readonly BulkItemDeclarationBuilder<TGameContext, TItemId> builder;

            public MovementCostBulkItemBuilder(BulkItemDeclarationBuilder<TGameContext, TItemId> builder)
            {
                this.builder = builder;
            }

            public BulkItemDeclarationBuilder<TGameContext, TItemId> AsPointCost<TMovementMode>(TMovementMode m, DistanceCalculation c, float unitCost, int preference = 0)
                where TMovementMode : IMovementMode
            {
                this.builder.WithTrait(new MovementPointCostBulkItemTrait<TGameContext, TItemId, TMovementMode>(m, c, unitCost, preference));
                this.builder.WithTrait(new PathfindingMovementCostFactorsTrait<TGameContext, TItemId>());
                return builder;
            }
            public BulkItemDeclarationBuilder<TGameContext, TItemId> AsVelocity<TMovementMode>(TMovementMode m, DistanceCalculation c, float unitCost, int preference = 0)
                where TMovementMode : IMovementMode
            {
                this.builder.WithTrait(new MovementVelocityBulkItemTrait<TGameContext, TItemId, TMovementMode>(m, c, unitCost, preference));
                this.builder.WithTrait(new PathfindingMovementCostFactorsTrait<TGameContext, TItemId>());
                return builder;
            }
        }

    }
}