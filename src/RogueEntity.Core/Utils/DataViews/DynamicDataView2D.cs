using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using MessagePack;
using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Utils.DataViews
{
    [DataContract]
    [MessagePackObject]
    public class DynamicDataView2D<T> : IDynamicDataView2D<T>
    {
        public event EventHandler<DynamicDataView2DEventArgs<T>> ViewCreated;
        public event EventHandler<DynamicDataView2DEventArgs<T>> ViewExpired;

        [DataMember(Order = 0)]
        [Key(0)]
        readonly int tileSizeX;

        [DataMember(Order = 1)]
        [Key(1)]
        readonly int tileSizeY;

        [DataMember(Order = 2)]
        [Key(2)]
        readonly Dictionary<Position2D, BoundedDataView<T>> index;

        [DataMember(Order = 4)]
        [Key(4)]
        long currentTime;

        [DataMember(Order = 5)]
        [Key(5)]
        int offsetX;

        [DataMember(Order = 6)]
        [Key(6)]
        int offsetY;

        [IgnoreDataMember]
        [IgnoreMember]
        public int TileSizeX => tileSizeX;

        [IgnoreDataMember]
        [IgnoreMember]
        public int TileSizeY => tileSizeY;

        [IgnoreDataMember]
        [IgnoreMember]
        public int OffsetX => offsetX;

        [IgnoreDataMember]
        [IgnoreMember]
        public int OffsetY => offsetY;

        [IgnoreDataMember]
        [IgnoreMember]
        Rectangle activeBounds;

        public DynamicDataView2D(DynamicDataViewConfiguration config) : this(config.OffsetX, config.OffsetY, config.TileSizeX, config.TileSizeY)
        {
        }

        public DynamicDataView2D(int tileSizeX, int tileSizeY) : this(0, 0, tileSizeX, tileSizeY)
        {
        }

        public DynamicDataView2D(int offsetX, int offsetY, int tileSizeX, int tileSizeY)
        {
            if (tileSizeX <= 0) throw new ArgumentException(nameof(tileSizeX));
            if (tileSizeY <= 0) throw new ArgumentException(nameof(tileSizeY));

            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.tileSizeX = tileSizeX;
            this.tileSizeY = tileSizeY;
            this.index = new Dictionary<Position2D, BoundedDataView<T>>();
        }

        [SerializationConstructor]
        protected DynamicDataView2D(int tileSizeX,
                                  int tileSizeY,
                                  [NotNull] Dictionary<Position2D, BoundedDataView<T>> index,
                                  long currentTime,
                                  int offsetX,
                                  int offsetY)
        {
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.tileSizeX = tileSizeX;
            this.tileSizeY = tileSizeY;
            this.index = index ?? throw new ArgumentNullException(nameof(index));
            this.currentTime = currentTime;
        }

        public Rectangle GetActiveBounds()
        {
            if (index.Count == 0) return default;
            if (activeBounds.Width != 0 && activeBounds.Height != 0)
            {
                return activeBounds;
            }

            Rectangle r = default;

            foreach (var k in index.Values)
            {
                if (r.Width == 0 || r.Height == 0)
                {
                    r = k.Bounds;
                }
                else
                {
                    r = r.GetUnion(k.Bounds);
                }
            }

            activeBounds = r;
            return r;
        }

        public BufferList<Rectangle> GetActiveTiles(BufferList<Rectangle> data = null)
        {
            data = BufferList.PrepareBuffer(data);

            foreach (var k in index.Values)
            {
                data.Add(k.Bounds);
            }

            return data;
        }

        public void Clear()
        {
            foreach (var e in index.Values)
            {
                e.Clear();
            }
        }

        public void Fill(in T data)
        {
            foreach (var e in index.Values)
            {
                e.Fill(in data);
            }
        }

        public BoundedDataView<T> GetOrCreateData(int x, int y)
        {
            if (TryGetDataInternal(x, y, out BoundedDataView<T> rawData))
            {
                return rawData;
            }

            var dx = DataViewPartitions.TileSplit(x, offsetX, tileSizeX);
            var dy = DataViewPartitions.TileSplit(y, offsetY, tileSizeY);
            
            var data = CreateDataViewInternal(dx, dy);
            
            ViewCreated?.Invoke(this, new DynamicDataView2DEventArgs<T>(data));
            return data;
        }

        BoundedDataView<T> CreateDataViewInternal(int dx, int dy)
        {
            var bounds = new Rectangle(dx * tileSizeX + offsetX, dy * tileSizeY + offsetY, tileSizeX, tileSizeY);
            var data = new BoundedDataView<T>(bounds);
            index[new Position2D(dx, dy)] = data;
            activeBounds = default;
            return data;
        }

        public bool TryGetData(int x, int y, out IReadOnlyBoundedDataView<T> raw)
        {
            if (TryGetDataInternal(x, y, out BoundedDataView<T> t))
            {
                raw = t;
                return true;
            }

            raw = default;
            return false;
        }

        public bool TryGetWriteAccess(int x, int y, out IBoundedDataView<T> raw, DataViewCreateMode mode = DataViewCreateMode.Nothing)
        {
            if (TryGetDataInternal(x, y, out BoundedDataView<T> t))
            {
                raw = t;
                return true;
            }

            if (mode == DataViewCreateMode.Nothing)
            {
                raw = default;
                return false;
            }
            
            var dx = DataViewPartitions.TileSplit(x, offsetX, tileSizeX);
            var dy = DataViewPartitions.TileSplit(y, offsetY, tileSizeY);
            var data = CreateDataViewInternal(dx, dy);
            
            ViewCreated?.Invoke(this, new DynamicDataView2DEventArgs<T>(data));

            raw = data;
            return true;
        }

        public bool TryGetRawAccess(int x, int y, out IBoundedDataViewRawAccess<T> raw)
        {
            if (TryGetDataInternal(x, y, out BoundedDataView<T> t))
            {
                raw = t;
                return true;
            }

            raw = default;
            return false;
        }

        public bool TrySet(int x, int y, in T value)
        {
            var data = GetOrCreateData(x, y);
            return data.TrySet(x, y, value);
        }

        bool TryGetDataInternal(int x, int y, out BoundedDataView<T> data)
        {
            var dx = DataViewPartitions.TileSplit(x, offsetX, tileSizeX);
            var dy = DataViewPartitions.TileSplit(y, offsetY, tileSizeY);
            
            if (!index.TryGetValue(new Position2D(dx, dy), out data))
            {
                return false;
            }

            return true;
        }

        public bool TryGet(int x, int y, out T result)
        {
            if (TryGetDataInternal(x, y, out BoundedDataView<T> data))
            {
                return data.TryGet(x, y, out result);
            }

            result = default;
            return false;
        }

        public ref T TryGetForUpdate(int x, int y, ref T defaultValue, out bool success, DataViewCreateMode mode = DataViewCreateMode.Nothing)
        {
            if (TryGetWriteAccess(x, y, out IBoundedDataView<T> data, mode))
            {
                return ref data.TryGetForUpdate(x, y, ref defaultValue, out success);
            }

            success = false;
            return ref defaultValue;
            
        }

        [IgnoreDataMember]
        [IgnoreMember]
        public T this[int x, int y]
        {
            get
            {
                if (TryGet(x, y, out var result))
                {
                    return result;
                }

                return default;
            }
            set
            {
                TrySet(x, y, in value);
            }
        }

        [IgnoreDataMember]
        [IgnoreMember]
        public T this[Position2D pos]
        {
            get
            {
                return this[pos.X, pos.Y];
            }
            set
            {
                this[pos.X, pos.Y] = value;
            }
        }
    }
}