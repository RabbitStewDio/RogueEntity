using System;
using JetBrains.Annotations;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Receptors.InfraVision;

namespace RogueEntity.Core.Sensing.Receptors.Heat
{
    public class SingleLevelHeatDirectionMap: IHeatMap
    {
        readonly IHeatPhysicsConfiguration heatPhysics;
        readonly SingleLevelSenseDirectionMapData<TemperatureSense, TemperatureSense> backend;

        public SingleLevelHeatDirectionMap([NotNull] IHeatPhysicsConfiguration heatPhysics,
                                           SingleLevelSenseDirectionMapData<TemperatureSense, TemperatureSense> backend)
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