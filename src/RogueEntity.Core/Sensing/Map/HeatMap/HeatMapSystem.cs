using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.InfraVision;
using RogueEntity.Core.Sensing.Sources.Heat;

namespace RogueEntity.Core.Sensing.Map.HeatMap
{
    public class HeatMapSystem: SenseMappingSystemBase<VisionSense, HeatSourceDefinition>, IHeatMap
    {
        readonly IHeatPhysicsConfiguration heatPhysics;

        public HeatMapSystem([NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider, 
                             [NotNull] IHeatPhysicsConfiguration heatPhysics,
                             ISenseDataBlitter blitterFactory = null) : base(senseCacheProvider, blitterFactory)
        {
            this.heatPhysics = heatPhysics ?? throw new ArgumentNullException(nameof(heatPhysics));
        }

        
        public Temperature GetEnvironmentTemperature(int z)
        {
            return heatPhysics.GetEnvironmentTemperature(z);
        }
        
    }
}