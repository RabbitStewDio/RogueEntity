using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;
using RogueEntity.Core.Sensing.Sources.Light;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    public class VisionSenseReceptorPhysicsConfiguration: IVisionSenseReceptorPhysicsConfiguration
    {
        readonly ShadowPropagationResistanceDataSource dataSource;
        
        public VisionSenseReceptorPhysicsConfiguration(ILightPhysicsConfiguration lightPhysics, ShadowPropagationResistanceDataSource dataSource = null)
        {
            this.dataSource = dataSource ?? new ShadowPropagationResistanceDataSource();
            VisionPhysics = new FullStrengthSensePhysics(lightPhysics.LightPhysics);
        }

        public ISensePhysics VisionPhysics { get; }
        
        public ISensePropagationAlgorithm CreateVisionSensorPropagationAlgorithm()
        {
            return new ShadowPropagationAlgorithm(VisionPhysics, dataSource);
        }
    }
}