using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Receptors.Smell
{
    public class SmellSenseReceptorPhysicsConfiguration: ISmellSenseReceptorPhysicsConfiguration
    {
        readonly FloodFillWorkingDataSource dataSource;

        public SmellSenseReceptorPhysicsConfiguration(ISmellPhysicsConfiguration lightPhysics, FloodFillWorkingDataSource dataSource)
        {
            this.dataSource = dataSource;
            SmellPhysics = new FullStrengthSensePhysics(lightPhysics.SmellPhysics);
        }

        public ISensePhysics SmellPhysics { get; }
        
        public ISensePropagationAlgorithm CreateSmellSensorPropagationAlgorithm()
        {
            return new FloodFillPropagationAlgorithm(SmellPhysics, dataSource);
        }
    }
}