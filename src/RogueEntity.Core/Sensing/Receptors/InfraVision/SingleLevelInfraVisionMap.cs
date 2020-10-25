using System;
using JetBrains.Annotations;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Map.HeatMap;

namespace RogueEntity.Core.Sensing.Receptors.InfraVision
{
    public class SingleLevelInfraVisionMap: IHeatMap
    {
        readonly IHeatPhysicsConfiguration heatPhysics;
        readonly SingleLevelSenseDirectionMapData<VisionSense, TemperatureSense> backend;

        public SingleLevelInfraVisionMap([NotNull] IHeatPhysicsConfiguration heatPhysics,
                                  SingleLevelSenseDirectionMapData<VisionSense, TemperatureSense> backend)
        {
            this.heatPhysics = heatPhysics ?? throw new ArgumentNullException(nameof(heatPhysics));
            this.backend = backend;
        }

        public bool TryGetSenseData(int z, out ISenseDataView intensities)
        {
            return backend.TryGetIntensity(z, out intensities);
        }
        
        public Temperature GetEnvironmentTemperature(int z)
        {
            return heatPhysics.GetEnvironmentTemperature(z);
        }
    }
}