using System;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing.Common;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Receptors.InfraVision
{
    public class SingleLevelInfraVisionMap: IHeatMap
    {
        readonly IInfraVisionSenseReceptorPhysicsConfiguration heatPhysics;
        readonly SingleLevelSenseDirectionMapData<VisionSense, TemperatureSense> backend;

        public SingleLevelInfraVisionMap(IInfraVisionSenseReceptorPhysicsConfiguration heatPhysics,
                                  SingleLevelSenseDirectionMapData<VisionSense, TemperatureSense> backend)
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