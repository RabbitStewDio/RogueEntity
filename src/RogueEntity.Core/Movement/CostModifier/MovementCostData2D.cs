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
        public readonly IReadOnlyDynamicDataView2D<DirectionalityInformation> InboundDirections;
        public readonly IReadOnlyDynamicDataView2D<DirectionalityInformation> OutboundDirections;

        public MovementCostData2D(in MovementCost movementCost,
                                  IReadOnlyDynamicDataView2D<float> costs,
                                  IReadOnlyDynamicDataView2D<DirectionalityInformation> inboundDirections,
                                  IReadOnlyDynamicDataView2D<DirectionalityInformation> outboundDirections)
        {
            BaseCost = movementCost.Cost;
            MovementType = movementCost.MovementMode ?? throw new ArgumentNullException(nameof(movementCost.MovementMode));
            Costs = costs ?? throw new ArgumentNullException(nameof(costs));
            InboundDirections = inboundDirections ?? throw new ArgumentNullException(nameof(inboundDirections));
            OutboundDirections = outboundDirections ?? throw new ArgumentNullException(nameof(outboundDirections));
        }

        public MovementCostData2D(in IMovementMode movementMode,
                                  IReadOnlyDynamicDataView2D<float> costs,
                                  IReadOnlyDynamicDataView2D<DirectionalityInformation> inboundDirections,
                                  IReadOnlyDynamicDataView2D<DirectionalityInformation> outboundDirections)
        {
            BaseCost = 1;
            MovementType = movementMode ?? throw new ArgumentNullException(nameof(movementMode));
            Costs = costs ?? throw new ArgumentNullException(nameof(costs));
            InboundDirections = inboundDirections ?? throw new ArgumentNullException(nameof(inboundDirections));
            OutboundDirections = outboundDirections ?? throw new ArgumentNullException(nameof(outboundDirections));
        }
    }
}