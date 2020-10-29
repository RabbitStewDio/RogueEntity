using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    public interface ILightPhysicsConfiguration
    {
        ISensePhysics LightPhysics { get; }
        ISensePropagationAlgorithm CreateLightPropagationAlgorithm();
    }
}