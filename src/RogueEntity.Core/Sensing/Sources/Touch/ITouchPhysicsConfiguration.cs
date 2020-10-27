namespace RogueEntity.Core.Sensing.Common.Physics
{
    public interface ITouchPhysicsConfiguration
    {
        ISensePhysics TouchPhysics { get; }
        ISensePropagationAlgorithm CreateTouchPropagationAlgorithm();
    }
}