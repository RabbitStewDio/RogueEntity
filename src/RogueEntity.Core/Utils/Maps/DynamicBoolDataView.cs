using System;
using System.Collections.Generic;
using System.Threading;
using GoRogue;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Utils.Maps
{
    public class DynamicBoolDataView : IReadOnlyView2D<bool>
    {
        public event EventHandler<DynamicBoolDataViewEventArgs> ViewCreated;
        public event EventHandler<DynamicBoolDataViewEventArgs> ViewExpired;

        readonly int tileSizeX;
        readonly int tileSizeY;
        readonly Dictionary<Position2D, TrackedDataView> index;
        readonly List<Position2D> expired;
        long currentTime;

        public int TileSizeX
        {
            get { return tileSizeX; }
        }

        public int TileSizeY
        {
            get { return tileSizeY; }
        }

        public DynamicBoolDataView(int tileSizeX, int tileSizeY)
        {
            if (tileSizeX <= 0) throw new ArgumentException(nameof(tileSizeX));
            if (tileSizeY <= 0) throw new ArgumentException(nameof(tileSizeY));

            this.tileSizeX = tileSizeX;
            this.tileSizeY = tileSizeY;
            index = new Dictionary<Position2D, TrackedDataView>();
            expired = new List<Position2D>();
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
            var dx = x / tileSizeX;
            var dy = y / tileSizeY;
            if (!index.TryGetValue(new Position2D(dx, dy), out var data))
            {
                data = new TrackedDataView(new Rectangle(dx * tileSizeX, dy * tileSizeY, tileSizeX, tileSizeY), currentTime);
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
            var dx = x / tileSizeX;
            var dy = y / tileSizeY;
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
        }

        class TrackedDataView : BoundedBoolDataView
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