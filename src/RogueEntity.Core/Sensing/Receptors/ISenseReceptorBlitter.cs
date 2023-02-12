using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Rectangle = RogueEntity.Core.Utils.Rectangle;

namespace RogueEntity.Core.Sensing.Receptors
{
    public interface ISenseReceptorBlitter
    {
        void Blit(Rectangle bounds,
                  GridPosition2D sensePosition,
                  GridPosition2D receptorPosition,
                  SenseSourceData senseSource,
                  BoundedDataView<float> receptorSenseIntensities,
                  BoundedDataView<byte> receptorSenseDirections);
    }

    /// <summary>
    ///   A blitter that copies sense data that spreads based on
    ///   air channels (Noise, Smell, Hot Air).
    /// 
    ///   Tagging interface as Dependency Injection in C# is primitive.
    /// </summary>
    public interface IDirectionalSenseReceptorBlitter : ISenseReceptorBlitter
    {
    }

    /// <summary>
    ///   A blitter that copies sense data that spreads as radiation
    ///  (light, heat source radiation) 
    /// 
    ///   Tagging interface as Dependency Injection in C# is primitive.
    /// </summary>
    public interface IRadiationSenseReceptorBlitter : ISenseReceptorBlitter
    {
    }
}