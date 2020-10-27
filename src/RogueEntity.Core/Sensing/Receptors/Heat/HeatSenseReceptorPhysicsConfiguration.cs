using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;

namespace RogueEntity.Core.Sensing.Receptors.Heat
{
    public class HeatSenseReceptorPhysicsConfiguration : IHeatSenseReceptorPhysicsConfiguration
    {
        readonly IHeatPhysicsConfiguration heatPhysics;

        public HeatSenseReceptorPhysicsConfiguration(IHeatPhysicsConfiguration heatPhysics)
        {
            this.heatPhysics = heatPhysics;
            HeatPhysics = new FullStrengthSensePhysics(heatPhysics.HeatPhysics);
        }

        public Temperature GetEnvironmentTemperature(int z)
        {
            return heatPhysics.GetEnvironmentTemperature(z);
        }

        public ISensePhysics HeatPhysics { get; }
        
        public ISensePropagationAlgorithm CreateHeatSensorPropagationAlgorithm()
        {
            return new ShadowPropagationAlgorithm(HeatPhysics);
        }
    }
}