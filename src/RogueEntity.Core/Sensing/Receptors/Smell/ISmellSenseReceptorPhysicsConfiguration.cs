using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Receptors.Smell
{
    public interface ISmellSenseReceptorPhysicsConfiguration
    {
        ISensePhysics SmellPhysics { get; }
        ISensePropagationAlgorithm CreateSmellSensorPropagationAlgorithm();
    }
}