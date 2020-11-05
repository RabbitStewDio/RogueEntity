using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Receptors.Touch
{
    public class TouchSenseReceptorPhysicsConfiguration : ITouchReceptorPhysicsConfiguration
    {
        readonly FloodFillWorkingDataSource dataSource;

        public TouchSenseReceptorPhysicsConfiguration(ISensePhysics physics, 
                                                      FloodFillWorkingDataSource dataSource = null)
        {
            this.dataSource = dataSource ?? new FloodFillWorkingDataSource();
            TouchPhysics = new FullStrengthSensePhysics(physics);
        }

        public ISensePhysics TouchPhysics { get; }
        
        public ISensePropagationAlgorithm CreateTouchSensorPropagationAlgorithm()
        {
            return new FloodFillPropagationAlgorithm(TouchPhysics, dataSource);
        }
    }
}