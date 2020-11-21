using RogueEntity.Core.Directionality;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.Pathfinding
{
    public interface IPathFinderSourceBackend
    {
        void RegisterMovementSource(IMovementMode movementMode,
                                    IReadOnlyDynamicDataView3D<float> cost,
                                    IReadOnlyDynamicDataView3D<DirectionalityInformation> direction);
    }
}