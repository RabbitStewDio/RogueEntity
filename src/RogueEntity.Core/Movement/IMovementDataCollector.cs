using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement
{
    public interface IMovementDataCollector
    {
        void RegisterMovementSource(IMovementMode movementMode,
                                    IReadOnlyDynamicDataView3D<float> cost,
                                    IReadOnlyDynamicDataView3D<DirectionalityInformation> inboundDirection,
                                    IReadOnlyDynamicDataView3D<DirectionalityInformation> outboundDirection);
    }
}