using System;
using JetBrains.Annotations;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Resistance.Maps;

namespace RogueEntity.Core.Sensing.Receptors.Heat
{
    public class TemperatureSenseSystem: SenseReceptorSystemBase<TemperatureSense, TemperatureSense>
    {
        public TemperatureSenseSystem([NotNull] Lazy<ISensePropertiesSource> senseProperties,
                                      [NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider,
                                      [NotNull] ISensePhysics physics,
                                      [NotNull] ISensePropagationAlgorithm sensePropagationAlgorithm) : base(senseProperties, senseCacheProvider, physics, sensePropagationAlgorithm)
        {
        }
    }
}