using System;
using JetBrains.Annotations;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    public class LightPhysicsConfiguration: ILightPhysicsConfiguration
    {
        public LightPhysicsConfiguration([NotNull] ISensePhysics lightPhysics)
        {
            LightPhysics = lightPhysics ?? throw new ArgumentNullException(nameof(lightPhysics));
        }

        public ISensePhysics LightPhysics { get; }
        
        public ISensePropagationAlgorithm CreateLightPropagationAlgorithm()
        {
            return new ShadowPropagationAlgorithm(LightPhysics);
        }
    }
}