namespace RogueEntity.Core.Sensing.Common.Physics
{
    public interface ILightPhysicsConfiguration
    {
        ISensePhysics LightPhysics { get; }
        ISensePropagationAlgorithm CreateLightPropagationAlgorithm();
    }
}