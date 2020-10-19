using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    public interface IBrightnessMap: ISenseDirectionMap
    {
        bool TryGetLightColors(int z, out IReadOnlyView2D<Color> colorMap);
    }
}