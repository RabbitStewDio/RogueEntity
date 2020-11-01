using System;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Sensing.Sources.Heat;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Receptors.Heat
{
    public class HeatReceptorSystem : SenseReceptorSystemBase<TemperatureSense, TemperatureSense>
    {
        public HeatReceptorSystem([NotNull] Lazy<ISensePropertiesSource> senseProperties,
                                  [NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider,
                                  [NotNull] Lazy<IGlobalSenseStateCacheProvider> globalSenseCacheProvider,
                                  [NotNull] Lazy<ITimeSource> timeSource,
                                  [NotNull] IHeatSenseReceptorPhysicsConfiguration physics) :
            base(senseProperties, senseCacheProvider, globalSenseCacheProvider, timeSource, physics.HeatPhysics, physics.CreateHeatSensorPropagationAlgorithm())
        {
        }

        protected override IReadOnlyView2D<float> CreateSensoryResistanceView(IReadOnlyView2D<SensoryResistance> resistanceMap)
        {
            return new HeatResistanceView(resistanceMap);
        }
    }
}