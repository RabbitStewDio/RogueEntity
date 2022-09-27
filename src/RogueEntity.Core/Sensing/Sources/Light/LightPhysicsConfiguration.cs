using System;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    public class LightPhysicsConfiguration: ILightPhysicsConfiguration
    {
        readonly ShadowPropagationResistanceDataSource dataSource;
        
        public LightPhysicsConfiguration(ISensePhysics lightPhysics, 
                                         ShadowPropagationResistanceDataSource? dataSource = null)
        {
            LightPhysics = lightPhysics ?? throw new ArgumentNullException(nameof(lightPhysics));
            this.dataSource = dataSource ?? new ShadowPropagationResistanceDataSource();
        }

        public ISensePhysics LightPhysics { get; }
        
        public ISensePropagationAlgorithm CreateLightPropagationAlgorithm()
        {
            return new ShadowPropagationAlgorithm(LightPhysics, dataSource);
        }
    }
}