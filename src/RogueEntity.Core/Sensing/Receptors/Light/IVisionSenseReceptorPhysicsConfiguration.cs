using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    public interface IVisionSenseReceptorPhysicsConfiguration
    {
        public ISensePhysics VisionPhysics { get; }
        public ISensePropagationAlgorithm CreateVisionSensorPropagationAlgorithm();
    }
}