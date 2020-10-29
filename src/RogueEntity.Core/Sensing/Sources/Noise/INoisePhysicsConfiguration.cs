using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public interface INoisePhysicsConfiguration
    {
        ISensePhysics NoisePhysics { get; }
        ISensePropagationAlgorithm CreateNoisePropagationAlgorithm();
    }
}