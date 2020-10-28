using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;

namespace RogueEntity.Core.Sensing.Receptors.Noise
{
    public class NoiseSenseReceptorPhysicsConfiguration: INoiseSenseReceptorPhysicsConfiguration
    {
        readonly FloodFillWorkingDataSource dataSource;

        public NoiseSenseReceptorPhysicsConfiguration(INoisePhysicsConfiguration lightPhysics, FloodFillWorkingDataSource dataSource)
        {
            this.dataSource = dataSource;
            NoisePhysics = new FullStrengthSensePhysics(lightPhysics.NoisePhysics);
        }

        public ISensePhysics NoisePhysics { get; }
        
        public ISensePropagationAlgorithm CreateNoiseSensorPropagationAlgorithm()
        {
            return new FloodFillPropagationAlgorithm(NoisePhysics, dataSource);
        }
    }
}