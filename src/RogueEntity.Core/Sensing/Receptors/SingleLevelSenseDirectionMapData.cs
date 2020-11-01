using System.Runtime.Serialization;
using MessagePack;
using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Receptors
{
    public static class SingleLevelSenseDirectionMapData
    {
        public static SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense> Create<TReceptorSense, TSourceSense>()
        {
            return new SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense>(int.MinValue, new SenseDataMap());
        }
    }
    
    [DataContract]
    [MessagePackObject]
    public readonly struct SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense>
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public int Z { get; }
        
        [DataMember(Order = 1)]
        [Key(1)]
        public SenseDataMap SenseMap { get; }

        [SerializationConstructor]
        public SingleLevelSenseDirectionMapData(int z, SenseDataMap senseMap)
        {
            SenseMap = senseMap;
            Z = z;
        }

        public bool TryGetIntensity(int z, out IDynamicSenseDataView2D intensities)
        {
            if (z == this.Z)
            {
                intensities = SenseMap;
                return true;
            }

            intensities = default;
            return false;
        }

        public SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense> WithDisabledState()
        {
            return new SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense>(int.MinValue, SenseMap);
        }

        public SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense> WithLevel(int z)
        {
            return new SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense>(z, SenseMap);
        }
    }
}