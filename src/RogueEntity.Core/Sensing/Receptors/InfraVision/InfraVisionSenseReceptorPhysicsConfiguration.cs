using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;

namespace RogueEntity.Core.Sensing.Receptors.InfraVision
{
    public class InfraVisionSenseReceptorPhysicsConfiguration: IInfraVisionSenseReceptorPhysicsConfiguration
    {
        readonly IHeatPhysicsConfiguration heatPhysics;

        public InfraVisionSenseReceptorPhysicsConfiguration(IHeatPhysicsConfiguration heatPhysics)
        {
            this.heatPhysics = heatPhysics;
            InfraVisionPhysics = new FullStrengthSensePhysics(heatPhysics.HeatPhysics);
        }

        public Temperature GetEnvironmentTemperature(int z)
        {
            return heatPhysics.GetEnvironmentTemperature(z);
        }

        public ISensePhysics InfraVisionPhysics { get; }
        
        public ISensePropagationAlgorithm CreateInfraVisionSensorPropagationAlgorithm()
        {
            return new ShadowPropagationAlgorithm(InfraVisionPhysics);
        }
    }
}