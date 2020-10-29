using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils.Maps;
using Rectangle = RogueEntity.Core.Utils.Rectangle;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    [EntityComponent]
    [MessagePackObject]
    [DataContract]
    public class SingleLevelBrightnessMap: IBrightnessMap
    {
        [DataMember(Order = 0)]
        [Key(0)]
        SingleLevelSenseDirectionMapData<VisionSense, VisionSense> backend;
        
        [DataMember(Order = 1)]
        [Key(1)]
        public BoundedDataView<Color> RawColorData { get; private set; }

        [SerializationConstructor]
        public SingleLevelBrightnessMap(SingleLevelSenseDirectionMapData<VisionSense, VisionSense> backend, BoundedDataView<Color> rawColorData)
        {
            this.backend = backend;
            RawColorData = rawColorData;
        }

        public bool TryGetLightColors(int z, out IReadOnlyView2D<Color> colorData)
        {
            if (RawColorData != null && z == this.backend.Z)
            {
                colorData = RawColorData;
                return true;
            }

            colorData = default;
            return false;
        }

        public bool TryGetSenseData(int z, out ISenseDataView intensities)
        {
            return backend.TryGetIntensity(z, out intensities);
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