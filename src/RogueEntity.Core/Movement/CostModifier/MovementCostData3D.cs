using System;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.CostModifier
{
    public readonly struct MovementCostData3D
    {
        public readonly MovementCost MovementCost;
        public readonly IReadOnlyDynamicDataView3D<float> Costs;
        public readonly IReadOnlyDynamicDataView3D<DirectionalityInformation> Directions;

        public MovementCostData3D(in MovementCost movementCost,
                                  IReadOnlyDynamicDataView3D<float> costs,
                                  IReadOnlyDynamicDataView3D<DirectionalityInformation> directions)
        {
            MovementCost = movementCost;
            Costs = costs ?? throw new ArgumentNullException(nameof(costs));
            Directions = directions ?? throw new ArgumentNullException(nameof(directions));
        }
    }
}