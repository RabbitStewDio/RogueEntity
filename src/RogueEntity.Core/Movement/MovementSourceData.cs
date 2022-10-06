using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Utils.DataViews;
using System;

namespace RogueEntity.Core.Movement;

public readonly struct MovementSourceData
{
    public readonly IMovementMode MovementMode;
    public readonly IReadOnlyDynamicDataView3D<float> Costs;
    public readonly IReadOnlyDynamicDataView3D<DirectionalityInformation> InboundDirections;
    public readonly IReadOnlyDynamicDataView3D<DirectionalityInformation> OutboundDirections;

    public MovementSourceData(IMovementMode movementMode,
                              IReadOnlyDynamicDataView3D<float> costs,
                              IReadOnlyDynamicDataView3D<DirectionalityInformation> inboundDirections,
                              IReadOnlyDynamicDataView3D<DirectionalityInformation> outboundDirections)
    {
        MovementMode = movementMode ?? throw new ArgumentNullException(nameof(movementMode));
        Costs = costs ?? throw new ArgumentNullException(nameof(costs));
        InboundDirections = inboundDirections ?? throw new ArgumentNullException(nameof(inboundDirections));
        OutboundDirections = outboundDirections ?? throw new ArgumentNullException(nameof(outboundDirections));
    }
}