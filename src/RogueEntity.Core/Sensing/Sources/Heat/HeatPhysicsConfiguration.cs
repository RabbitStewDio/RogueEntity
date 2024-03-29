using System;
using System.Collections.Generic;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;

namespace RogueEntity.Core.Sensing.Sources.Heat
{
    public class HeatPhysicsConfiguration: IHeatPhysicsConfiguration
    {
        readonly ShadowPropagationResistanceDataSource dataSource;
        readonly Dictionary<int, Temperature> environmentTemperatures;
        readonly Temperature fallbackTemperature;

        public HeatPhysicsConfiguration(ISensePhysics heatPhysics,
                                        Temperature fallbackTemperature,
                                        ShadowPropagationResistanceDataSource? dataSource = null)
        {
            this.HeatPhysics = heatPhysics ?? throw new ArgumentNullException(nameof(heatPhysics));
            this.fallbackTemperature = fallbackTemperature;
            this.dataSource = dataSource ?? new ShadowPropagationResistanceDataSource();
            this.environmentTemperatures = new Dictionary<int, Temperature>();
        }

        public ISensePhysics HeatPhysics { get; }

        public void DefineEnvironmentTemperature(int z, Temperature t)
        {
            environmentTemperatures[z] = t;
        }
        
        public Temperature GetEnvironmentTemperature(int z)
        {
            if (environmentTemperatures.TryGetValue(z, out var t))
            {
                return t;
            }

            return fallbackTemperature;
        }

        public ISensePropagationAlgorithm CreateHeatPropagationAlgorithm()
        {
            return new ShadowPropagationAlgorithm(HeatPhysics, dataSource);
        }
    }
}