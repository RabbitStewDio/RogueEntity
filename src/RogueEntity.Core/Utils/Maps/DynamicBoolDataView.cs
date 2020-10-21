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
    public class DynamicBoolDataView : IReadOnlyView2D<bool>
    {
        public event EventHandler<DynamicBoolDataViewEventArgs> ViewCreated;
        public event EventHandler<DynamicBoolDataViewEventArgs> ViewExpired;

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

        public DynamicBoolDataView(int tileSizeX, int tileSizeY) : this(0, 0, tileSizeX, tileSizeY)
        {
            
        }
        
        public DynamicBoolDataView(int offsetX, int offsetY, int tileSizeX, int tileSizeY)
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
        internal DynamicBoolDataView(int tileSizeX, int tileSizeY, 
                                     [NotNull] Dictionary<Position2D, TrackedDataView> index,
                                     [NotNull] List<Position2D> expired, 
                                     long currentTime, 
                                     int offsetX, int offsetY)
        {
            this.tileSizeX = tileSizeX;
            this.tileSizeY = tileSizeY;
            this.index = index ?? throw new ArgumentNullException(nameof(index));
            this.expired = expired ?? throw new ArgumentNullException(nameof(expired));
            this.currentTime = currentTime;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
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
                if (e.AnyValueSet())
                {
                    e.MarkUsedForWriting();
                    e.Clear();
                }
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
                    ViewExpired?.Invoke(this, new DynamicBoolDataViewEventArgs(e));
                    expired.Add(entry.Key);
                }
            }

            foreach (var e in expired)
            {
                index.Remove(e);
            }
        }

        public bool Any(int x, int y)
        {
            if (TryGetData(x, y, out TrackedDataView data))
            {
                return data.Any(x, y);
            }

            return false;
        }
        
        public BoundedBoolDataView GetOrCreateData(int x, int y)
        {
            var dx = MapPartitions.TileSplit(x, offsetX, tileSizeX);
            var dy = MapPartitions.TileSplit(y, offsetX, tileSizeY);
            if (!index.TryGetValue(new Position2D(dx, dy), out var data))
            {
                data = new TrackedDataView(new Rectangle(dx * tileSizeX + offsetX, dy * tileSizeY + offsetY, tileSizeX, tileSizeY), currentTime);
                index[new Position2D(dx, dy)] = data;
                ViewCreated?.Invoke(this, new DynamicBoolDataViewEventArgs(data));
            }
            else
            {
                data.MarkUsedForWriting();
            }

            return data;
        }

        public bool TrySet(int x, int y, bool value)
        {
            var data = GetOrCreateData(x, y);
            return data[x,y] = value;
        }

        public bool TryGetData(int x, int y, out BoundedBoolDataView data)
        {
            if (TryGetData(x, y, out TrackedDataView v))
            {
                data = v;
                return true;
            }

            data = default;
            return false;
        }
        
        bool TryGetData(int x, int y, out TrackedDataView data)
        {
            var dx = MapPartitions.TileSplit(x, offsetX, tileSizeX);
            var dy = MapPartitions.TileSplit(y, offsetX, tileSizeY);
            if (!index.TryGetValue(new Position2D(dx, dy), out data))
            {
                return false;
            }

            data.MarkUsedForReading();
            return true;
        }

        public bool TryGet(int x, int y, out bool result)
        {
            if (TryGetData(x, y, out TrackedDataView data))
            {
                result = data[x, y];
                return true;
            }

            result = default;
            return false;
        }

        public bool this[int x, int y]
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
                TrySet(x, y, value);
            }
        }

        internal class TrackedDataView : BoundedBoolDataView
        {
            long currentTime;
            public long LastUsed { get; private set; }
            int usedForReading;
            int usedForWriting;

            public TrackedDataView(in Rectangle bounds, long time) : base(in bounds)
            {
                currentTime = time;
                LastUsed = time;
                usedForReading = 0;
                usedForWriting = 0;
            }

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

            public bool IsUsedForReading => usedForReading != 0;
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