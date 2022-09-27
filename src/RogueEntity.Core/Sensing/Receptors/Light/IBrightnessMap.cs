using RogueEntity.Core.Utils.DataViews;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    public interface IBrightnessMap: ISenseDirectionMap
    {
        bool TryGetLightColors(int z, [MaybeNullWhen(false)] out IReadOnlyView2D<Color> colorMap);
    }
}