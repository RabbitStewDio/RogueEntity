using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Receptors.Touch
{
    public interface ITouchReceptorPhysicsConfiguration
    {
        ITouchPhysicsConfiguration SourcePhysics { get; }
        ISensePhysics TouchPhysics { get; }
        ISensePropagationAlgorithm CreateTouchSensorPropagationAlgorithm();
    }
}