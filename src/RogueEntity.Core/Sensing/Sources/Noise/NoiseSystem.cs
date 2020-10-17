using System;
using JetBrains.Annotations;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Resistance.Maps;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public class NoiseSystem: SenseSystemBase<NoiseSense, NoiseSourceDefinition>
    {
        public NoiseSystem([NotNull] Lazy<ISensePropertiesSource> senseProperties, 
                           [NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider, 
                           [NotNull] ISensePropagationAlgorithm sensePropagationAlgorithm, 
                           [NotNull] ISensePhysics physics, 
                           ISenseDataBlitter blitterFactory = null) : base(senseProperties, senseCacheProvider, sensePropagationAlgorithm, physics, blitterFactory)
        {
        }
    }
}