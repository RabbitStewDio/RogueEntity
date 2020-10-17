using GoRogue;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Map.Light;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    public class SingleLevelBrightnessMap: IBrightnessMap
    {
        public BoundedDataView<Color> RawData { get; private set; }
        public SenseDataMap SenseMap { get; }
        public int Z { get; set; }

        public SingleLevelBrightnessMap()
        {
            Z = int.MinValue;
            SenseMap = new SenseDataMap();
        }

        public SingleLevelBrightnessMap(int z, BoundedDataView<Color> rawData)
        {
            this.RawData = rawData;
            this.Z = z;
        }

        public bool TryGetLightColors(int z, out IReadOnlyView2D<Color> colorData)
        {
            if (RawData != null && z == this.Z)
            {
                colorData = RawData;
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
            if (RawData != null)
            {
                RawData.Resize(bounds);
                return RawData;
            }
            
            RawData = new BoundedDataView<Color>(bounds);
            return RawData;
        }
    }
}