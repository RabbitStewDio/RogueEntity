using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing
{
    public interface ISmellPhysicsConfiguration
    {
        ISensePhysics SmellPhysics { get; }
        ISensePropagationAlgorithm CreateSmellPropagationAlgorithm();
    }
}