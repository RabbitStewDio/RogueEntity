using System;
using JetBrains.Annotations;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    /// <summary>
    ///   Performs light calculations by collecting all active lights and aggregating their affected areas into a global brightness map.
    /// </summary>
    public class LightSystem : SenseSystemBase<VisionSense, LightSourceDefinition>
    {

        public LightSystem([NotNull] Lazy<ISensePropertiesSource> senseProperties,
                           [NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider,
                           [NotNull] ISensePropagationAlgorithm sensePropagationAlgorithm,
                           [NotNull] ILightPhysicsConfiguration lightPhysics,
                           ISenseDataBlitter blitterFactory = null): base(senseProperties, senseCacheProvider, sensePropagationAlgorithm, lightPhysics.LightPhysics, blitterFactory)
        {
        }
        
        protected override IReadOnlyView2D<float> CreateSensoryResistanceView(IReadOnlyView2D<SensoryResistance> resistanceMap)
        {
            return new LightResistanceView(resistanceMap);
        }
    }
}