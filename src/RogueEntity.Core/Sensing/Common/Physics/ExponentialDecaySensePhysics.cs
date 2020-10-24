using System;
using GoRogue;
using JetBrains.Annotations;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Sensing.Common.Physics
{
    /// <summary>
    ///   a sense with a non-linear fall-off. This will create brightness in the (large)
    ///   center with a gradual decline towards the edge. This is a light mechanics used
    ///   by many 2D top-down games.
    ///
    ///   Unlike the inverse square lights used in real physics, this is a simple parabolic
    ///   formula that works well for comic style lights that produces a satisfyingly large
    ///   radiation zone around lights with a gradual, non-linear fall off towards the edges.
    /// </summary>
    public class ExponentialDecaySensePhysics: ISensePhysics
    {
        public ExponentialDecaySensePhysics(DistanceCalculation distanceMeasurement)
        {
            DistanceMeasurement = distanceMeasurement;
        }

        public DistanceCalculation DistanceMeasurement { get; }

        public float SignalRadiusForIntensity(float intensity)
        {
            return (float) Math.Sqrt(intensity);
        }

        public float SignalStrengthAtDistance(float distance, float maxRadius)
        {
            var fn = (distance / maxRadius);
            return (1 - (fn * fn)) * maxRadius;
        }
    }
}