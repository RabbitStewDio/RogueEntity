using System;
using JetBrains.Annotations;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Receptors.Temperature;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Sources.Heat
{
    public class HeatSystem : SenseSystemBase<TemperatureSense, HeatSourceDefinition>, IHeatMap
    {
        [NotNull] readonly IHeatPhysicsConfiguration heatPhysics;

        public HeatSystem([NotNull] Lazy<ISensePropertiesSource> senseProperties,
                          [NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider,
                          [NotNull] ISensePropagationAlgorithm sensePropagationAlgorithm,
                          [NotNull] IHeatPhysicsConfiguration heatPhysics,
                          ISenseDataBlitter blitterFactory = null) : 
            base(senseProperties, senseCacheProvider, sensePropagationAlgorithm, blitterFactory)
        {
            this.heatPhysics = heatPhysics;
        }

        protected override SenseSourceData RefreshSenseState<TPosition>(HeatSourceDefinition definition, TPosition pos, IReadOnlyView2D<float> resistanceView, SenseSourceData data)
        {
            var environmentTemperature = heatPhysics.GetEnvironmentTemperature(pos.GridZ);
            var senseDefinition = definition.SenseDefinition;
            
            var intensityRelative = senseDefinition.Intensity - environmentTemperature.ToCelsius();
            var radius = heatPhysics.HeatPhysics.SignalRadiusForIntensity(Math.Abs(intensityRelative));

            var defRel = definition.WithSenseSource(senseDefinition.WithIntensity(intensityRelative, radius));
            return base.RefreshSenseState(defRel, pos, resistanceView, data);
        }

        public bool TryGetHeatData(int z, out ISenseDataView heatMap)
        {
            return TryGetSenseData(z, out heatMap);
        }
    }
}