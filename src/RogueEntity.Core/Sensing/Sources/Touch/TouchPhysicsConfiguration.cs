using System;
using JetBrains.Annotations;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;

namespace RogueEntity.Core.Sensing.Sources.Touch
{
    public class TouchPhysicsConfiguration: ITouchPhysicsConfiguration
    {
        readonly ShadowPropagationResistanceDataSource dataSource;

        public TouchPhysicsConfiguration([NotNull] ISensePhysics touchPhysics,
                                         ShadowPropagationResistanceDataSource dataSource = null)
        {
            TouchPhysics = touchPhysics ?? throw new ArgumentNullException(nameof(touchPhysics));
            this.dataSource = dataSource ?? new ShadowPropagationResistanceDataSource();
        }

        public ISensePhysics TouchPhysics { get; }
        
        public ISensePropagationAlgorithm CreateTouchPropagationAlgorithm()
        {
            return new ShadowPropagationAlgorithm(TouchPhysics, dataSource);
        }
    }
}