using System;
using JetBrains.Annotations;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Sources.Smell
{
    public class SmellSystem: SenseSystemBase<SmellSense, SmellSourceDefinition>
    {
        public SmellSystem([NotNull] Lazy<ISensePropertiesSource> senseProperties,
                           [NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider, 
                           [NotNull] ISensePropagationAlgorithm sensePropagationAlgorithm, 
                           [NotNull] ISensePhysics physics, 
                           ISenseDataBlitter blitterFactory = null) : base(senseProperties, senseCacheProvider, sensePropagationAlgorithm, physics, blitterFactory)
        {
        }

        protected override IReadOnlyView2D<float> CreateSensoryResistanceView(IReadOnlyView2D<SensoryResistance> resistanceMap)
        {
            return new SmellResistanceView(resistanceMap);
        }
    }
}