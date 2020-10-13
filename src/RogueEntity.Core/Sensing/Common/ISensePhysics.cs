using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Common
{
    public interface ISensePhysics
    {
        /// <summary>
        ///   Calculates a maximum effective range for a given sense source with
        ///   the given intensity. 
        /// </summary>
        /// <param name="intensity">The intensity at the source.</param>
        /// <returns>The maximum effective radius for the area affected by the sense source.</returns>
        public float SignalRadiusForIntensity(float intensity);
        
        /// <summary>
        ///   Calculates the strength factor of a sense signal given a current distance from the source
        ///   and a maximum radius for a sense source,
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="maxRadius"></param>
        /// <returns></returns>
        public float SignalStrengthAtDistance(float distance, float maxRadius);
    }

    /// <summary>
    ///   A simple, minecraft style sense source. The intensity of the light is taken as active
    ///   radius, and the signal strength decays linearly over the distance.  
    /// </summary>
    public class LinearDecaySensePhysics: ISensePhysics
    {
        public static readonly LinearDecaySensePhysics Instance = new LinearDecaySensePhysics();
        
        public float SignalRadiusForIntensity(float intensity)
        {
            return intensity;
        }

        public float SignalStrengthAtDistance(float distance, float maxRadius)
        {
            return ((maxRadius - distance) / maxRadius).Clamp(0, 1);
        }
    }
}