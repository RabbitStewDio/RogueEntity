using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using GoRogue;
using MessagePack;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Map.Light;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    [EntityComponent]
    [MessagePackObject]
    [DataContract]
    public class SingleLevelBrightnessMap: IBrightnessMap
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public int Z { get; set; }
        [DataMember(Order = 1)]
        [Key(1)]
        public SenseDataMap SenseMap { get; }
        [DataMember(Order = 2)]
        [Key(2)]
        public BoundedDataView<Color> RawColorData { get; private set; }

        public SingleLevelBrightnessMap()
        {
            Z = int.MinValue;
            SenseMap = new SenseDataMap();
        }

        [SerializationConstructor]
        public SingleLevelBrightnessMap(int z, SenseDataMap senseMap, BoundedDataView<Color> rawColorData)
        {
            Z = z;
            SenseMap = senseMap;
            RawColorData = rawColorData;
        }

        public bool TryGetLightColors(int z, out IReadOnlyView2D<Color> colorData)
        {
            if (RawColorData != null && z == this.Z)
            {
                colorData = RawColorData;
                return true;
            }

            colorData = default;
            return false;
        }

        public bool TryGetLightIntensity(int z, out ISenseDataView intensities)
        {
            if (z == this.Z)
            {
                intensities = SenseMap;
                return true;
            }

            intensities = default;
            return false;
        }

        public BoundedDataView<Color> CreateColorMap(Rectangle bounds)
        {
            if (RawColorData != null)
            {
                RawColorData.Resize(bounds);
                return RawColorData;
            }
            
            RawColorData = new BoundedDataView<Color>(bounds);
            return RawColorData;
        }
    }
}