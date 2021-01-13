using RogueEntity.Core.Directionality;
using RogueEntity.Core.Utils.DataViews;
using System.Collections.Generic;

namespace RogueEntity.Core.Movement
{
    public interface IMovementDataCollector
    {
        void RegisterMovementSource(IMovementMode movementMode,
                                    IReadOnlyDynamicDataView3D<float> cost,
                                    IReadOnlyDynamicDataView3D<DirectionalityInformation> inboundDirection,
                                    IReadOnlyDynamicDataView3D<DirectionalityInformation> outboundDirection);
    }

    public interface IMovementDataProvider
    {
        IReadOnlyDictionary<IMovementMode, MovementSourceData> MovementCosts { get; }
    }
}