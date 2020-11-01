using System;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Sources.Heat
{
    /// <summary>
    ///   Computes a heat sense data map. The data is relative to the local environment temperature
    ///   to allow eay comparison of relative intensities instead of having to deal with large
    ///   numbers on all occasions. This makes heat sense scales comparable with vision or other sense
    ///   sources. 
    /// </summary>
    public class HeatSystem : SenseSystemBase<TemperatureSense, HeatSourceDefinition>
    {
        [NotNull] readonly IHeatPhysicsConfiguration heatPhysics;

        public HeatSystem([NotNull] Lazy<ISensePropertiesSource> senseProperties,
                          [NotNull] Lazy<IGlobalSenseStateCacheProvider> senseCacheProvider,
                          [NotNull] Lazy<ITimeSource> timeSource,
                          [NotNull] ISenseStateCacheControl senseCacheControl,
                          [NotNull] ISensePropagationAlgorithm sensePropagationAlgorithm,
                          [NotNull] IHeatPhysicsConfiguration heatPhysics) :
            base(senseProperties, senseCacheProvider, timeSource, senseCacheControl, sensePropagationAlgorithm, heatPhysics.HeatPhysics)
        {
            this.heatPhysics = heatPhysics ?? throw new ArgumentNullException(nameof(heatPhysics));
        }

        protected override IReadOnlyView2D<float> CreateSensoryResistanceView(IReadOnlyView2D<SensoryResistance> resistanceMap)
        {
            return new HeatResistanceView(resistanceMap);
        }

        protected override float ComputeIntensity(in SenseSourceDefinition sd, in Position p)
        {
            var localTemperature = heatPhysics.GetEnvironmentTemperature(p.GridZ).ToKelvin();
            var itemTemperature = sd.Intensity;
            return itemTemperature - localTemperature;
        }

        protected override SenseSourceData RefreshSenseState<TPosition>(in HeatSourceDefinition definition,
                                                                        float intensity,
                                                                        in TPosition pos,
                                                                        IReadOnlyView2D<float> resistanceView,
                                                                        SenseSourceData data)
        {
            var environmentTemperature = heatPhysics.GetEnvironmentTemperature(pos.GridZ);
            var senseDefinition = definition.SenseDefinition;

            var envKelvin = environmentTemperature.ToKelvin();
            var intensityRelative = senseDefinition.Intensity - envKelvin;
            var result = base.RefreshSenseState(definition, intensityRelative, pos, resistanceView, data);
            return result;
        }
    }
}