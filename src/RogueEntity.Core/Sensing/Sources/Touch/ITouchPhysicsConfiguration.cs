using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Sources.Touch
{
    public interface ITouchPhysicsConfiguration
    {
        ISensePhysics TouchPhysics { get; }
        ISensePropagationAlgorithm CreateTouchPropagationAlgorithm();
    }
}