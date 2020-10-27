namespace RogueEntity.Core.Sensing.Common.Physics
{
    public interface ISmellPhysicsConfiguration
    {
        ISensePhysics SmellPhysics { get; }
        ISensePropagationAlgorithm CreateSmellPropagationAlgorithm();
    }
}