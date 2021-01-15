using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Movement.Cost
{
    public static class MovementCostItemDeclarations
    {
        public static MovementCostBulkItemBuilder<TItemId> WithMovement<TItemId>(this BulkItemDeclarationBuilder<TItemId> builder)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return new MovementCostBulkItemBuilder<TItemId>(builder);
        }

        public static MovementCostReferenceItemBuilder<TItemId> WithMovement<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder)
            where TItemId : IEntityKey
        {
            return new MovementCostReferenceItemBuilder<TItemId>(builder);
        }

        public readonly struct MovementCostReferenceItemBuilder<TItemId>
            where TItemId : IEntityKey
        {
            readonly ReferenceItemDeclarationBuilder<TItemId> builder;

            public MovementCostReferenceItemBuilder(ReferenceItemDeclarationBuilder<TItemId> builder)
            {
                this.builder = builder;
            }

            public ReferenceItemDeclarationBuilder<TItemId> AsPointCost<TMovementMode>(TMovementMode m, DistanceCalculation c, float unitCost, int preference = 0)
                where TMovementMode : IMovementMode
            {
                this.builder.WithTrait(new MovementPointCostReferenceItemTrait<TItemId, TMovementMode>(m, c, unitCost, preference));
                this.builder.WithTrait(new PathfindingMovementCostFactorsTrait<TItemId>());
                return builder;
            }

            public ReferenceItemDeclarationBuilder<TItemId> AsVelocity<TMovementMode>(TMovementMode m, DistanceCalculation c, float unitCost, int preference = 0)
                where TMovementMode : IMovementMode
            {
                this.builder.WithTrait(new MovementVelocityReferenceItemTrait<TItemId, TMovementMode>(m, c, unitCost, preference));
                this.builder.WithTrait(new PathfindingMovementCostFactorsTrait<TItemId>());
                return builder;
            }
        }

        public readonly struct MovementCostBulkItemBuilder<TItemId>
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            readonly BulkItemDeclarationBuilder<TItemId> builder;

            public MovementCostBulkItemBuilder(BulkItemDeclarationBuilder<TItemId> builder)
            {
                this.builder = builder;
            }

            public BulkItemDeclarationBuilder<TItemId> AsPointCost<TMovementMode>(TMovementMode m, DistanceCalculation c, float unitCost, int preference = 0)
                where TMovementMode : IMovementMode
            {
                this.builder.WithTrait(new MovementPointCostBulkItemTrait<TItemId, TMovementMode>(m, c, unitCost, preference));
                this.builder.WithTrait(new PathfindingMovementCostFactorsTrait<TItemId>());
                return builder;
            }

            public BulkItemDeclarationBuilder<TItemId> AsVelocity<TMovementMode>(TMovementMode m, DistanceCalculation c, float unitCost, int preference = 0)
                where TMovementMode : IMovementMode
            {
                this.builder.WithTrait(new MovementVelocityBulkItemTrait<TItemId, TMovementMode>(m, c, unitCost, preference));
                this.builder.WithTrait(new PathfindingMovementCostFactorsTrait<TItemId>());
                return builder;
            }
        }
    }
}
