using System;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public class NoiseSystem: SenseSystemBase<NoiseSense, NoiseSourceDefinition>
    {
        public NoiseSystem([NotNull] Lazy<ISensePropertiesSource> senseProperties, 
                           [NotNull] Lazy<IGlobalSenseStateCacheProvider> senseCacheProvider, 
                           [NotNull] Lazy<ITimeSource> timeSource,
                           [NotNull] ISenseStateCacheControl senseCacheControl,
                           [NotNull] ISensePropagationAlgorithm sensePropagationAlgorithm, 
                           [NotNull] ISensePhysics physics) : base(senseProperties, senseCacheProvider, timeSource, senseCacheControl, sensePropagationAlgorithm, physics)
        {
        }
        
        protected override IReadOnlyView2D<float> CreateSensoryResistanceView(IReadOnlyView2D<SensoryResistance> resistanceMap)
        {
            return new NoiseResistanceView(resistanceMap);
        }
    }
}