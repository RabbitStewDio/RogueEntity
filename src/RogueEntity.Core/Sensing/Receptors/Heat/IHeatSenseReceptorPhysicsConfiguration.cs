using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Receptors.Heat
{
    public interface IHeatSenseReceptorPhysicsConfiguration
    {
        public Temperature GetEnvironmentTemperature(int z);
        public ISensePhysics HeatPhysics { get; }
        public ISensePropagationAlgorithm CreateHeatSensorPropagationAlgorithm();
    }
}