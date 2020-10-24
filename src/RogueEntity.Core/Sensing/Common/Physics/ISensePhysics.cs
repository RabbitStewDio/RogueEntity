using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Sensing.Common.Physics
{
    public interface ISensePhysics
    {
        DistanceCalculation DistanceMeasurement { get; }
        
        /// <summary>
        ///   Calculates a maximum effective range for a given sense source with
        ///   the given intensity. 
        /// </summary>
        /// <param name="intensity">The intensity at the source.</param>
        /// <returns>The maximum effective radius for the area affected by the sense source.</returns>
        float SignalRadiusForIntensity(float intensity);
        
        /// <summary>
        ///   Calculates the strength factor of a sense signal given a current distance from the source
        ///   and a maximum radius for a sense source,
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="maxRadius"></param>
        /// <returns></returns>
        float SignalStrengthAtDistance(float distance, float maxRadius);
    }
}