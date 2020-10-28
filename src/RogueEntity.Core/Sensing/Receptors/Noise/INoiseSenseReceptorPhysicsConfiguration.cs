using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Receptors.Noise
{
    public interface INoiseSenseReceptorPhysicsConfiguration
    {
        ISensePhysics NoisePhysics { get; }
        ISensePropagationAlgorithm CreateNoiseSensorPropagationAlgorithm();
    }
}