using System;
using RogueEntity.Api.Time;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Resistance.Directions;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Sources.Heat
{
    /// <summary>
    ///   Computes a heat sense data map. The data is relative to the local environment temperature
    ///   to allow eay comparison of relative intensities instead of having to deal with large
    ///   numbers on all occasions. This makes heat sense scales comparable with vision or other sense
    ///   sources. 
    /// </summary>
    public class HeatSourceSystem : SenseSourceSystem<TemperatureSense, HeatSourceDefinition>
    {
        readonly IHeatPhysicsConfiguration heatPhysics;

        public HeatSourceSystem(Lazy<IReadOnlyDynamicDataView3D<float>> senseProperties,
                                Lazy<IGlobalSenseStateCacheProvider> senseCacheProvider,
                                Lazy<ITimeSource> timeSource,
                                ISensoryResistanceDirectionView<TemperatureSense> directionalitySystem,
                                ISenseStateCacheControl senseCacheControl,
                                ISensePropagationAlgorithm sensePropagationAlgorithm,
                                IHeatPhysicsConfiguration heatPhysics) :
            base(senseProperties, senseCacheProvider, timeSource, directionalitySystem, senseCacheControl, sensePropagationAlgorithm, heatPhysics.HeatPhysics)
        {
            this.heatPhysics = heatPhysics ?? throw new ArgumentNullException(nameof(heatPhysics));
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
                                                                        IReadOnlyDynamicDataView2D<float> resistanceView,
                                                                        IReadOnlyDynamicDataView2D<DirectionalityInformation> directionView,
                                                                        SenseSourceData data)
        {
            var environmentTemperature = heatPhysics.GetEnvironmentTemperature(pos.GridZ);
            var senseDefinition = definition.SenseDefinition;

            var envKelvin = environmentTemperature.ToKelvin();
            var intensityRelative = senseDefinition.Intensity - envKelvin;
            var result = base.RefreshSenseState(definition, intensityRelative, pos, resistanceView, directionView, data);
            return result;
        }
    }
}