using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Movement.Cost
{
    public readonly struct PathfindingMovementCostFactors
    {
        public readonly ReadOnlyListWrapper<MovementCost> MovementCosts;

        public PathfindingMovementCostFactors(ReadOnlyListWrapper<MovementCost> movementCosts)
        {
            MovementCosts = movementCosts;
        }
    }
}