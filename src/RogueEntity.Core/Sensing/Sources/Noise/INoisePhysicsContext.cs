namespace RogueEntity.Core.Sensing.Sources
{
    public interface INoisePhysicsContext
    {
        public float NoiseSignalRadiusForIntensity(float intensity);
        public float NoiseSignalStrengthAtDistance(float distance, float maxRadius);
    }
}