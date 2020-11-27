using System;
using JetBrains.Annotations;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Sensing.Common.Physics
{
    public class FullStrengthSensePhysics : ISensePhysics
    {
        readonly ISensePhysics physics;

        public FullStrengthSensePhysics([NotNull] ISensePhysics physics)
        {
            this.physics = physics ?? throw new ArgumentNullException(nameof(physics));
        }

        public AdjacencyRule AdjacencyRule => physics.AdjacencyRule;

        public DistanceCalculation DistanceMeasurement => physics.DistanceMeasurement;

        public float SignalRadiusForIntensity(float intensity)
        {
            return physics.SignalRadiusForIntensity(intensity);
        }

        public float SignalStrengthAtDistance(float distance, float maxRadius)
        {
            if (distance > maxRadius) return 0;
            return 1;
        }
    }
}