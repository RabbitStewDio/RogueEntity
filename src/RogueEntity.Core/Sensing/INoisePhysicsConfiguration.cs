using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing
{
    public interface INoisePhysicsConfiguration
    {
        ISensePhysics NoisePhysics { get; }
        ISensePropagationAlgorithm CreateNoisePropagationAlgorithm();
    }
}