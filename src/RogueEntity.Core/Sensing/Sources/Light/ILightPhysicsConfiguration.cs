namespace RogueEntity.Core.Sensing.Sources.Light
{
    public interface ILightPhysicsConfiguration
    {
        public float LightSignalRadiusForIntensity(float intensity);
        public float LightSignalStrengthAtDistance(float distance, float maxRadius);
    }
}