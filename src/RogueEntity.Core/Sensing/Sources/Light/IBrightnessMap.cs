using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    public interface IBrightnessMap
    {
        bool TryGetLightData(int z, out ISenseDataView brightnessMap);
    }
}