using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Sources.Smell
{
    public interface ISmellPhysicsConfiguration
    {
        ISensePhysics SmellPhysics { get; }
        ISensePropagationAlgorithm CreateSmellPropagationAlgorithm();
    }
}