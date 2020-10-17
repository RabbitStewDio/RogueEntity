using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Map.HeatMap
{
    /// <summary>
    ///   Returns a heat map view of the current level. The heat map stores
    ///   temperatures as intensities using absolute temperature values
    ///   (Kelvin). You can make them relative to the current environmental
    ///   temperature if needed.
    /// </summary>
    public interface IHeatMap
    {
        bool TryGetHeatIntensity(int z, out ISenseDataView brightnessMap);
        Temperature GetEnvironmentTemperature(int z);
    }
}