using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using MessagePack;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Common
{
    [DataContract]
    [MessagePackObject]
    public class SenseDataMap : ISenseDataView
    {
        [DataMember(Order = 0)]
        [Key(0)]
        readonly DynamicDataView<float> sensitivityData;
        [DataMember(Order = 1)]
        [Key(1)]
        readonly DynamicDataView<byte> directionData;

        public SenseDataMap(int tileWidth = 64, int tileHeight = 64)
        {
            sensitivityData = new DynamicDataView<float>(tileWidth, tileHeight);
            directionData = new DynamicDataView<byte>(tileWidth, tileHeight);
        }

        [SerializationConstructor]
        public SenseDataMap([NotNull] DynamicDataView<float> sensitivityData, [NotNull] DynamicDataView<byte> directionData)
        {
            this.sensitivityData = sensitivityData ?? throw new ArgumentNullException(nameof(sensitivityData));
            this.directionData = directionData ?? throw new ArgumentNullException(nameof(directionData));
        }

        public void Clear()
        {
            sensitivityData.ClearData();
            directionData.ClearData();
        }

        public bool TryQuery(int x, int y, out float intensity, out SenseDirection directionality, out SenseDataFlags flags)
        {
            if (sensitivityData.TryGet(x, y, out intensity) &&
                directionData.TryGet(x, y, out var raw))
            {
                var d = new SenseDirectionStore(raw);
                directionality = d.Direction;
                flags = d.Flags;
                return true;
            }

            directionality = default;
            flags = default;
            return false;
        }

        public float QueryBrightness(int x, int y)
        {
            if (sensitivityData.TryGet(x, y, out var result))
            {
                return result;
            }

            return default;
        }

        public SenseDirectionStore QueryDirection(int x, int y)
        {
            if (directionData.TryGet(x, y, out var result))
            {
                return new SenseDirectionStore(result);
            }

            return default;
        }

        public int TileSizeX
        {
            get { return sensitivityData.TileSizeX; }
        }

        public int TileSizeY
        {
            get { return sensitivityData.TileSizeY; }
        }

        public void FetchRawData(int x, int y, out BoundedDataView<float> brightness, out BoundedDataView<byte> directions)
        {
            brightness = sensitivityData.GetOrCreateData(x, y);
            directions = directionData.GetOrCreateData(x, y);
        }
    }
}