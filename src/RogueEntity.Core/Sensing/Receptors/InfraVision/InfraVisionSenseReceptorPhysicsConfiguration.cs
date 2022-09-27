using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;
using RogueEntity.Core.Sensing.Sources.Heat;

namespace RogueEntity.Core.Sensing.Receptors.InfraVision
{
    public class InfraVisionSenseReceptorPhysicsConfiguration: IInfraVisionSenseReceptorPhysicsConfiguration
    {
        readonly IHeatPhysicsConfiguration heatPhysics;
        readonly ShadowPropagationResistanceDataSource dataSource;

        public InfraVisionSenseReceptorPhysicsConfiguration(IHeatPhysicsConfiguration heatPhysics,
                                                            ShadowPropagationResistanceDataSource? dataSource = null)
        {
            this.heatPhysics = heatPhysics;
            this.dataSource = dataSource ?? new ShadowPropagationResistanceDataSource();
            InfraVisionPhysics = new FullStrengthSensePhysics(heatPhysics.HeatPhysics);
        }

        public Temperature GetEnvironmentTemperature(int z)
        {
            return heatPhysics.GetEnvironmentTemperature(z);
        }

        public ISensePhysics InfraVisionPhysics { get; }
        
        public ISensePropagationAlgorithm CreateInfraVisionSensorPropagationAlgorithm()
        {
            return new ShadowPropagationAlgorithm(InfraVisionPhysics, dataSource);
        }
    }
}