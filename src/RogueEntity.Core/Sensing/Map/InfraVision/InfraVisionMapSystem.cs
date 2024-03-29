using System;
using RogueEntity.Api.Time;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing.Receptors.InfraVision;
using RogueEntity.Core.Sensing.Sources.Heat;

namespace RogueEntity.Core.Sensing.Map.InfraVision
{
    public class InfraVisionMapSystem : SenseMappingSystemBase<VisionSense, TemperatureSense, HeatSourceDefinition>, IHeatMap
    {
        readonly IInfraVisionSenseReceptorPhysicsConfiguration heatPhysics;

        public InfraVisionMapSystem(Lazy<ITimeSource> timeSource,
                                    IInfraVisionSenseReceptorPhysicsConfiguration heatPhysics,
                                    ISenseMapDataBlitter? blitterFactory = null) : base(timeSource, blitterFactory)
        {
            this.heatPhysics = heatPhysics ?? throw new ArgumentNullException(nameof(heatPhysics));
        }


        public Temperature GetEnvironmentTemperature(int z)
        {
            return heatPhysics.GetEnvironmentTemperature(z);
        }
    }
}