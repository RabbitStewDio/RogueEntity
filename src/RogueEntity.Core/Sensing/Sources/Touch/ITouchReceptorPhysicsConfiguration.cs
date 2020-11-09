using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Sources.Touch
{
    public interface ITouchReceptorPhysicsConfiguration
    {
        ISensePhysics TouchPhysics { get; }
        ISensePropagationAlgorithm CreateTouchSensorPropagationAlgorithm();
    }
}