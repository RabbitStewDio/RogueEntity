using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using MessagePack;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Map.HeatMap;

namespace RogueEntity.Core.Sensing.Receptors.InfraVision
{
    public class SingleLevelHeatMap: IHeatMap
    {
        readonly IHeatPhysicsConfiguration heatPhysics;
        readonly SingleLevelHeatMapData backend;

        public SingleLevelHeatMap([NotNull] IHeatPhysicsConfiguration heatPhysics,
                                  [NotNull] SingleLevelHeatMapData backend)
        {
            this.heatPhysics = heatPhysics ?? throw new ArgumentNullException(nameof(heatPhysics));
            this.backend = backend ?? throw new ArgumentNullException(nameof(backend));
        }

        public bool TryGetHeatIntensity(int z, out ISenseDataView intensities)
        {
            return backend.TryGetHeatIntensity(z, out intensities);
        }
        
        public Temperature GetEnvironmentTemperature(int z)
        {
            return heatPhysics.GetEnvironmentTemperature(z);
        }
    }

    [DataContract]
    [MessagePackObject]
    public class SingleLevelHeatMapData
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public int Z { get; set; }
        
        [DataMember(Order = 1)]
        [Key(1)]
        public SenseDataMap SenseMap { get; }

        public SingleLevelHeatMapData()
        {
            Z = int.MinValue;
            SenseMap = new SenseDataMap();
        }

        [SerializationConstructor]
        public SingleLevelHeatMapData(int z, SenseDataMap senseMap)
        {
            SenseMap = senseMap;
            Z = z;
        }

        public bool TryGetHeatIntensity(int z, out ISenseDataView intensities)
        {
            if (z == this.Z)
            {
                intensities = SenseMap;
                return true;
            }

            intensities = default;
            return false;
        }
    }
}