using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Map.Light
{
    public interface IBrightnessMap
    {
        bool TryGetLightIntensity(int z, out ISenseDataView brightnessMap);
        bool TryGetLightColors(int z, out IReadOnlyView2D<Color> colorMap);
    }
}