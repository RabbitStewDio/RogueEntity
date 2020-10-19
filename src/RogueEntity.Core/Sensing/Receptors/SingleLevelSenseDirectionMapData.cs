using System.Runtime.Serialization;
using MessagePack;
using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Receptors
{
    public static class SingleLevelSenseDirectionMapData
    {
        public static SingleLevelSenseDirectionMapData<TSense, TSource> Create<TSense, TSource>()
        {
            return new SingleLevelSenseDirectionMapData<TSense, TSource>(int.MinValue, new SenseDataMap());
        }
    }
    
    [DataContract]
    [MessagePackObject]
    public readonly struct SingleLevelSenseDirectionMapData<TSense, TSource>
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

        public bool TryGetIntensity(int z, out ISenseDataView intensities)
        {
            if (z == this.Z)
            {
                intensities = SenseMap;
                return true;
            }

            intensities = default;
            return false;
        }

        public SingleLevelSenseDirectionMapData<TSense, TSource> WithDisabledState()
        {
            return new SingleLevelSenseDirectionMapData<TSense, TSource>(int.MinValue, SenseMap);
        }

        public SingleLevelSenseDirectionMapData<TSense, TSource> WithLevel(int z)
        {
            return new SingleLevelSenseDirectionMapData<TSense, TSource>(z, SenseMap);
        }
    }
}