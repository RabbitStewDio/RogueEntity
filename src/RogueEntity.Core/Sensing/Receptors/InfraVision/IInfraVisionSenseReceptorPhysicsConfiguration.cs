using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Receptors.InfraVision
{
    public interface IInfraVisionSenseReceptorPhysicsConfiguration
    {
        public Temperature GetEnvironmentTemperature(int z);
        public ISensePhysics InfraVisionPhysics { get; }
        public ISensePropagationAlgorithm CreateInfraVisionSensorPropagationAlgorithm();
    }
}