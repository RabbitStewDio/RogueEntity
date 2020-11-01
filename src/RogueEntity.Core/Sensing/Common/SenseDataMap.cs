using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using MessagePack;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Common
{
    public class SenseDirectionStoreTileWrapper: IReadOnlyBoundedDataView<SenseDirectionStore>
    {
        readonly IReadOnlyBoundedDataView<byte> raw;

        public SenseDirectionStoreTileWrapper([NotNull] IReadOnlyBoundedDataView<byte> raw)
        {
            this.raw = raw ?? throw new ArgumentNullException(nameof(raw));
        }

        public bool TryGet(int x, int y, out SenseDirectionStore data)
        {
            if (raw.TryGet(x, y, out var dataRaw))
            {
                data = new SenseDirectionStore(dataRaw);
                return true;
            }

            data = default;
            return false;
        }

        public SenseDirectionStore this[int x, int y]
        {
            get { return new SenseDirectionStore(raw[x, y]); }
        }

        public Rectangle Bounds
        {
            get { return raw.Bounds; }
        }
    }
    
    [DataContract]
    [MessagePackObject]
    public class SenseDataMap : IDynamicSenseDataView2D
    {
        [DataMember(Order = 0)]
        [Key(0)]
        readonly DynamicDataView<float> sensitivityData;
        [DataMember(Order = 1)]
        [Key(1)]
        readonly DynamicDataView<byte> directionData;

        public SenseDataMap(int tileWidth = 64, int tileHeight = 64) : this(0, 0, tileWidth, tileHeight)
        {
        }
        
        public SenseDataMap(int offsetX, int offsetY, int tileWidth, int tileHeight)
        {
            sensitivityData = new DynamicDataView<float>(offsetX, offsetY, tileWidth, tileHeight);
            directionData = new DynamicDataView<byte>(offsetX, offsetY, tileWidth, tileHeight);
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

        public Rectangle GetActiveBounds()
        {
            return sensitivityData.GetActiveBounds();
        }

        public bool TryQuery(int x, int y, out float intensity, out SenseDirectionStore directionality)
        {
            if (sensitivityData.TryGet(x, y, out intensity) &&
                directionData.TryGet(x, y, out var raw))
            {
                var d = new SenseDirectionStore(raw);
                directionality = d;
                return true;
            }

            directionality = default;
            return false;
        }

        public bool TryStore(int x, int y, float intensity, SenseDirectionStore directions)
        {
            var r1 = sensitivityData.TrySet(x, y, intensity);
            var r2 = directionData.TrySet(x, y, directions.RawData);
            return r1 && r2;
        }
        
        public void Write(Position2D point,
                          Position2D origin,
                          float intensity, 
                          SenseDataFlags flags = SenseDataFlags.None)
        {
            var direction = point - origin;
            TryStore(point.X, point.Y, intensity, SenseDirectionStore.From(direction.X, direction.Y).With(flags));
        }

        public int OffsetX
        {
            get { return sensitivityData.OffsetX; }
        }

        public int OffsetY
        {
            get { return sensitivityData.OffsetY; }
        }

        public List<Rectangle> GetActiveTiles(List<Rectangle> data = null)
        {
            return sensitivityData.GetActiveTiles(data);
        }

        public int TileSizeX
        {
            get { return sensitivityData.TileSizeX; }
        }

        public int TileSizeY
        {
            get { return sensitivityData.TileSizeY; }
        }

        float IReadOnlyView2D<float>.this[int x, int y]
        {
            get { return sensitivityData[x, y]; }
        }

        SenseDirectionStore IReadOnlyView2D<SenseDirectionStore>.this[int x, int y]
        {
            get { return new SenseDirectionStore(directionData[x, y]); }
        }

        public bool TryGet(int x, int y, out float data)
        {
            return sensitivityData.TryGet(x, y, out data);
        }

        public bool TryGetData(int x, int y, out IReadOnlyBoundedDataView<float> raw)
        {
            return sensitivityData.TryGetData(x, y, out raw);
        }

        public bool TryGet(int x, int y, out SenseDirectionStore data)
        {
            if (directionData.TryGet(x, y, out var d))
            {
                data = new SenseDirectionStore(d);
                return true;
            }

            data = default;
            return false;
        }

        public bool TryGetData(int x, int y, out IReadOnlyBoundedDataView<SenseDirectionStore> raw)
        {
            if (directionData.TryGetData(x, y, out var rawBytes))
            {
                raw = new SenseDirectionStoreTileWrapper(rawBytes);
                return true;
            }

            raw = default;
            return false;
        }

        public void FetchRawData(int x, int y, out BoundedDataView<float> brightness, out BoundedDataView<byte> directions)
        {
            brightness = sensitivityData.GetOrCreateData(x, y);
            directions = directionData.GetOrCreateData(x, y);
        }
    }
}