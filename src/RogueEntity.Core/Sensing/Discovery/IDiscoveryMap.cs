using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Discovery
{
    public interface IDiscoveryMap
    {
        bool TryGetMap(int z, out IReadOnlyView2D<bool> data);
    }
}