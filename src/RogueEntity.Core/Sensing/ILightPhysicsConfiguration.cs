using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing
{
    public interface ILightPhysicsConfiguration
    {
        ISensePhysics LightPhysics { get; }
        ISensePropagationAlgorithm CreateLightPropagationAlgorithm();
    }
}