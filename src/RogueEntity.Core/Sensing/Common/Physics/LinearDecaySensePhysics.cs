using System;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Common.Physics
{
    /// <summary>
    ///   A simple, minecraft style sense source. The intensity of the light is taken as active
    ///   radius, and the signal strength decays linearly over the distance. This is the light
    ///   mechanics used by Minecraft.
    /// </summary>
    public class LinearDecaySensePhysics: ISensePhysics
    {
        public static LinearDecaySensePhysics For(DistanceCalculation c) => new LinearDecaySensePhysics(c);

        public LinearDecaySensePhysics(DistanceCalculation distanceMeasurement)
        {
            DistanceMeasurement = distanceMeasurement;
            AdjacencyRule = distanceMeasurement.AsAdjacencyRule();
        }

        public LinearDecaySensePhysics(DistanceCalculation distanceMeasurement, AdjacencyRule adjacencyRule)
        {
            DistanceMeasurement = distanceMeasurement;
            AdjacencyRule = adjacencyRule;
        }

        public AdjacencyRule AdjacencyRule { get; }
        
        public DistanceCalculation DistanceMeasurement { get; }
        
        public float SignalRadiusForIntensity(float intensity)
        {
            return Math.Abs(intensity);
        }

        public float SignalStrengthAtDistance(float distance, float maxRadius)
        {
            return ((maxRadius - distance) / maxRadius).Clamp(0, 1);
        }
    }
}