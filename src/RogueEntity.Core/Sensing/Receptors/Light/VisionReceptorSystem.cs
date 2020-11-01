using System;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    public class VisionReceptorSystem : SenseReceptorSystemBase<VisionSense, VisionSense>
    {
        public VisionReceptorSystem([NotNull] Lazy<ISensePropertiesSource> senseProperties,
                                    [NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider,
                                    [NotNull] Lazy<IGlobalSenseStateCacheProvider> globalSenseCacheProvider,
                                    [NotNull] Lazy<ITimeSource> timeSource,
                                    [NotNull] ISensePhysics physics,
                                    [NotNull] ISensePropagationAlgorithm sensePropagationAlgorithm) : 
            base(senseProperties, senseCacheProvider, globalSenseCacheProvider, timeSource, physics, sensePropagationAlgorithm)
        {
        }

        protected override IReadOnlyView2D<float> CreateSensoryResistanceView(IReadOnlyView2D<SensoryResistance> resistanceMap)
        {
            return new LightResistanceView(resistanceMap);
        }
    }
}