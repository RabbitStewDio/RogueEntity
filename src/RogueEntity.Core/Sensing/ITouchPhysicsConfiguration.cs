using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing
{
    public interface ITouchPhysicsConfiguration
    {
        ISensePhysics TouchPhysics { get; }
        ISensePropagationAlgorithm CreateTouchPropagationAlgorithm();
    }
}