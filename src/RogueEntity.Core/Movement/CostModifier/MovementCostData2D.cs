using System;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.CostModifier
{
    public readonly struct MovementCostData2D
    {
        public readonly IMovementMode MovementType;
        public readonly float BaseCost;
        public readonly IReadOnlyDynamicDataView2D<float> Costs;
        public readonly IReadOnlyDynamicDataView2D<DirectionalityInformation> Directions;

        public MovementCostData2D(in MovementCost movementCost,
                                  IReadOnlyDynamicDataView2D<float> costs,
                                  IReadOnlyDynamicDataView2D<DirectionalityInformation> directions)
        {
            BaseCost = movementCost.Cost;
            MovementType = movementCost.MovementMode ?? throw new ArgumentNullException(nameof(movementCost.MovementMode));
            Costs = costs ?? throw new ArgumentNullException(nameof(costs));
            Directions = directions ?? throw new ArgumentNullException(nameof(directions));
        }

        public MovementCostData2D(in IMovementMode movementMode,
                                  IReadOnlyDynamicDataView2D<float> costs,
                                  IReadOnlyDynamicDataView2D<DirectionalityInformation> directions)
        {
            BaseCost = 1;
            MovementType = movementMode ?? throw new ArgumentNullException(nameof(movementMode));
            Costs = costs ?? throw new ArgumentNullException(nameof(costs));
            Directions = directions ?? throw new ArgumentNullException(nameof(directions));
        }
    }
}