using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Discovery
{
    public interface IDiscoveryMap
    {
        bool TryGetMap(int z, out IReadOnlyView2D<bool> data);
    }
}