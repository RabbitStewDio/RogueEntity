using System;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Receptors.InfraVision;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Receptors.Heat
{
    public class SingleLevelHeatDirectionMap: IHeatMap
    {
        readonly IHeatSenseReceptorPhysicsConfiguration heatPhysics;
        readonly SingleLevelSenseDirectionMapData<TemperatureSense, TemperatureSense> backend;

        public SingleLevelHeatDirectionMap(IHeatSenseReceptorPhysicsConfiguration heatPhysics,
                                           SingleLevelSenseDirectionMapData<TemperatureSense, TemperatureSense> backend)
        {
            this.heatPhysics = heatPhysics ?? throw new ArgumentNullException(nameof(heatPhysics));
            this.backend = backend;
        }

        public bool TryGetSenseData(int z, [MaybeNullWhen(false)] out IDynamicSenseDataView2D intensities)
        {
            return backend.TryGetIntensity(z, out intensities);
        }
        
        public Temperature GetEnvironmentTemperature(int z)
        {
            return heatPhysics.GetEnvironmentTemperature(z);
        }

    }
}