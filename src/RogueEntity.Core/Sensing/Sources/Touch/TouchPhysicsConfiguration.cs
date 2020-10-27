using System;
using JetBrains.Annotations;
using RogueEntity.Core.Sensing.Common.ShadowCast;

namespace RogueEntity.Core.Sensing.Common.Physics
{
    public class TouchPhysicsConfiguration: ITouchPhysicsConfiguration
    {
        public TouchPhysicsConfiguration([NotNull] ISensePhysics touchPhysics)
        {
            TouchPhysics = touchPhysics ?? throw new ArgumentNullException(nameof(touchPhysics));
        }

        public ISensePhysics TouchPhysics { get; }
        
        public ISensePropagationAlgorithm CreateTouchPropagationAlgorithm()
        {
            return new ShadowPropagationAlgorithm(TouchPhysics);
        }
    }
}