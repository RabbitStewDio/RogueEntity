using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;

namespace RogueEntity.Core.Sensing.Receptors.Heat
{
    public class HeatSenseReceptorPhysicsConfiguration : IHeatSenseReceptorPhysicsConfiguration
    {
        readonly IHeatPhysicsConfiguration heatPhysics;
        readonly FloodFillWorkingDataSource dataSource;

        public HeatSenseReceptorPhysicsConfiguration(IHeatPhysicsConfiguration heatPhysics, FloodFillWorkingDataSource dataSource)
        {
            this.heatPhysics = heatPhysics;
            this.dataSource = dataSource;
            HeatPhysics = new FullStrengthSensePhysics(heatPhysics.HeatPhysics);
        }

        public Temperature GetEnvironmentTemperature(int z)
        {
            return heatPhysics.GetEnvironmentTemperature(z);
        }

        public ISensePhysics HeatPhysics { get; }
        
        public ISensePropagationAlgorithm CreateHeatSensorPropagationAlgorithm()
        {
            return new FloodFillPropagationAlgorithm(HeatPhysics, dataSource);
        }
    }
}