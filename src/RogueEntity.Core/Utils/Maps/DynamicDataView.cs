using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using GoRogue;
using JetBrains.Annotations;
using MessagePack;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Utils.Maps
{
    public interface IDynamicDataView2D<T> : IView2D<T>
    {
        int OffsetX { get; }
        int OffsetY { get; }
        int TileSizeX { get; }
        int TileSizeY { get; }

        bool TrySet(int x, int y, in T value);
        bool TryGet(int x, int y, out T result);

        List<Rectangle> GetActiveTiles(List<Rectangle> data = null);
        bool TryGetData(int x, int y, out IBoundedDataViewRawAccess<T> raw);
    }

    [DataContract]
    [MessagePackObject]
    public class DynamicDataView<T> : IDynamicDataView2D<T>
    {
        public event EventHandler<DynamicDataViewEventArgs<T>> ViewCreated;
        public event EventHandler<DynamicDataViewEventArgs<T>> ViewExpired;

        [DataMember(Order = 0)]
        [Key(0)]
        readonly int tileSizeX;

        [DataMember(Order = 1)]
        [Key(1)]
        readonly int tileSizeY;

        [DataMember(Order = 2)]
        [Key(2)]
        readonly Dictionary<Position2D, TrackedDataView> index;

        [DataMember(Order = 3)]
        [Key(3)]
        readonly List<Position2D> expired;

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

        public DynamicDataView(int tileSizeX, int tileSizeY) : this(0, 0, tileSizeX, tileSizeY)
        {
        }

        public DynamicDataView(int offsetX, int offsetY, int tileSizeX, int tileSizeY)
        {
            if (tileSizeX <= 0) throw new ArgumentException(nameof(tileSizeX));
            if (tileSizeY <= 0) throw new ArgumentException(nameof(tileSizeY));

            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.tileSizeX = tileSizeX;
            this.tileSizeY = tileSizeY;
            index = new Dictionary<Position2D, TrackedDataView>();
            expired = new List<Position2D>();
        }

        [SerializationConstructor]
        protected DynamicDataView(int tileSizeX,
                                  int tileSizeY,
                                  [NotNull] Dictionary<Position2D, TrackedDataView> index,
                                  [NotNull] List<Position2D> expired,
                                  long currentTime,
                                  int offsetX,
                                  int offsetY)
        {
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.tileSizeX = tileSizeX;
            this.tileSizeY = tileSizeY;
            this.index = index ?? throw new ArgumentNullException(nameof(index));
            this.expired = expired ?? throw new ArgumentNullException(nameof(expired));
            this.currentTime = currentTime;
        }

        public List<Rectangle> GetActiveTiles(List<Rectangle> data = null)
        {
            if (data == null)
            {
                data = new List<Rectangle>();
            }
            else
            {
                data.Clear();
            }

            foreach (var k in index.Values)
            {
                data.Add(k.Bounds);
            }

            return data;
        }

        public void PrepareFrame(long time)
        {
            currentTime = time;
            foreach (var e in index.Values)
            {
                e.MarkUnused(time);
            }
        }

        public void ClearData()
        {
            foreach (var e in index.Values)
            {
                e.MarkUsedForWriting();
                e.Clear();
            }
        }

        public void ExpireFrames(long age)
        {
            expired.Clear();

            foreach (var entry in index)
            {
                var e = entry.Value;
                e.MarkUsedAge();
                if ((currentTime - e.LastUsed) > age)
                {
                    ViewExpired?.Invoke(this, new DynamicDataViewEventArgs<T>(e));
                    expired.Add(entry.Key);
                }
            }

            foreach (var e in expired)
            {
                index.Remove(e);
            }
        }

        public BoundedDataView<T> GetOrCreateData(int x, int y)
        {
            if (TryGetDataInternal(x, y, out TrackedDataView rawData))
            {
                rawData.MarkUsedForWriting();
                return rawData;
            }

            var dx = MapPartitions.TileSplit(x, offsetX, tileSizeX);
            var dy = MapPartitions.TileSplit(y, offsetY, tileSizeY);
            var data = new TrackedDataView(new Rectangle(dx * tileSizeX + offsetX, dy * tileSizeY + offsetY, tileSizeX, tileSizeY), currentTime);
            data.MarkUsedForReading();
            data.MarkUsedForWriting();

            index[new Position2D(dx, dy)] = data;
            ViewCreated?.Invoke(this, new DynamicDataViewEventArgs<T>(data));
            return data;
        }

        public bool TryGetData(int x, int y, out IBoundedDataViewRawAccess<T> raw)
        {
            if (TryGetDataInternal(x, y, out TrackedDataView t))
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

        bool TryGetDataInternal(int x, int y, out TrackedDataView data)
        {
            var dx = MapPartitions.TileSplit(x, offsetX, tileSizeX);
            var dy = MapPartitions.TileSplit(y, offsetY, tileSizeY);
            if (!index.TryGetValue(new Position2D(dx, dy), out data))
            {
                return false;
            }

            data.MarkUsedForReading();
            return true;
        }

        public bool TryGet(int x, int y, out T result)
        {
            if (TryGetDataInternal(x, y, out TrackedDataView data))
            {
                return data.TryGet(x, y, out result);
            }

            result = default;
            return false;
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

        [DataContract]
        [MessagePackObject]
        protected class TrackedDataView : BoundedDataView<T>
        {
            [DataMember(Order = 2)]
            [Key(2)]
            long currentTime;

            [DataMember(Order = 3)]
            [Key(3)]
            int usedForReading;

            [DataMember(Order = 4)]
            [Key(4)]
            int usedForWriting;

            public TrackedDataView(in Rectangle bounds, long time) : base(in bounds)
            {
                currentTime = time;
                LastUsed = time;
                usedForReading = 0;
                usedForWriting = 0;
            }

            [SerializationConstructor]
            public TrackedDataView(Rectangle bounds, T[] data, long currentTime, int usedForReading, int usedForWriting, long lastUsed) : base(bounds, data)
            {
                this.currentTime = currentTime;
                this.usedForReading = usedForReading;
                this.usedForWriting = usedForWriting;
                LastUsed = lastUsed;
            }

            [DataMember(Order = 5)]
            [Key(5)]
            public long LastUsed { get; private set; }

            public void MarkUnused(long time)
            {
                lock (this)
                {
                    currentTime = time;
                    usedForReading = 0;
                    usedForWriting = 0;
                }
            }

            public void MarkUsedForReading()
            {
                Interlocked.CompareExchange(ref usedForReading, 1, 0);
            }

            public void MarkUsedForWriting()
            {
                Interlocked.CompareExchange(ref usedForReading, 1, 0);
                Interlocked.CompareExchange(ref usedForWriting, 1, 0);
            }

            [IgnoreDataMember]
            [IgnoreMember]
            public bool IsUsedForReading => usedForReading != 0;

            [IgnoreDataMember]
            [IgnoreMember]
            public bool IsUsedForWriting => usedForWriting != 0;

            public void MarkUsedAge()
            {
                lock (this)
                {
                    if (usedForReading == 1)
                    {
                        LastUsed = currentTime;
                    }
                }
            }
        }
    }
}