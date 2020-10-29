using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Sources.Touch;

namespace RogueEntity.Core.Sensing.Receptors.Touch
{
    public class TouchSenseReceptorPhysicsConfiguration : ITouchReceptorPhysicsConfiguration
    {
        readonly FloodFillWorkingDataSource dataSource;

        public TouchSenseReceptorPhysicsConfiguration(ITouchPhysicsConfiguration sourcePhysics, FloodFillWorkingDataSource dataSource)
        {
            this.dataSource = dataSource;
            SourcePhysics = sourcePhysics;
            TouchPhysics = new FullStrengthSensePhysics(SourcePhysics.TouchPhysics);
        }

        public ITouchPhysicsConfiguration SourcePhysics { get; }
        public ISensePhysics TouchPhysics { get; }
        public ISensePropagationAlgorithm CreateTouchSensorPropagationAlgorithm()
        {
            return new FloodFillPropagationAlgorithm(TouchPhysics, dataSource);
        }
    }
}