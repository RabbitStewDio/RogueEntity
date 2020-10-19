using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Sensing.Receptors.InfraVision
{
    /// <summary>
    ///   Returns a heat map view of the current level. The heat map stores
    ///   temperatures as intensities using absolute temperature values
    ///   (Kelvin). You can make them relative to the current environmental
    ///   temperature if needed.
    /// </summary>
    public interface IHeatMap: ISenseDirectionMap
    {
        Temperature GetEnvironmentTemperature(int z);
    }
}