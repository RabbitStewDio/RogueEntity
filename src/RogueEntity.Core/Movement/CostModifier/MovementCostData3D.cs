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
        public readonly IReadOnlyDynamicDataView3D<DirectionalityInformation> InboundDirections;
        public readonly IReadOnlyDynamicDataView3D<DirectionalityInformation> OutboundDirections;

        public MovementCostData3D(in MovementCost movementCost,
                                  IReadOnlyDynamicDataView3D<float> costs,
                                  IReadOnlyDynamicDataView3D<DirectionalityInformation> inboundDirections,
                                  IReadOnlyDynamicDataView3D<DirectionalityInformation> outboundDirections)
        {
            MovementCost = movementCost;
            Costs = costs ?? throw new ArgumentNullException(nameof(costs));
            InboundDirections = inboundDirections ?? throw new ArgumentNullException(nameof(inboundDirections));
            OutboundDirections = outboundDirections ?? throw new ArgumentNullException(nameof(outboundDirections));
        }

        public bool TryGetMovementData2D(int z, out MovementCostData2D costData)
        {
            if (Costs.TryGetView(z, out var costs) &&
                InboundDirections.TryGetView(z, out var inbound) &&
                OutboundDirections.TryGetView(z, out var outbound))
            {
                costData = new MovementCostData2D(MovementCost, costs, inbound, outbound);
                return true;
            }

            costData = default;
            return false;

        }
    }
}