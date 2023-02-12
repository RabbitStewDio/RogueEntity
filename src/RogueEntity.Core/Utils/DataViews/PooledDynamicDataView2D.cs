using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MessagePack;
using RogueEntity.Api.Utils;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Utils.DataViews
{
    [DataContract]
    [MessagePackObject]
    public class PooledDynamicDataView2D<T> : IDynamicDataView2D<T>, IPooledDataViewControl2D
    {
        public event EventHandler<DynamicDataView2DEventArgs<T>>? ViewChunkCreated;
        public event EventHandler<DynamicDataView2DEventArgs<T>>? ViewChunkExpired;

        readonly Dictionary<TileIndex, IPooledBoundedDataView<T>> index;

        long currentTime;
        readonly List<(TileIndex idx, IPooledBoundedDataView<T> view)> expired;
        Rectangle activeBounds;

        readonly IBoundedDataViewPool<T> pool;
        readonly DynamicDataViewConfiguration tileConfiguration;

        public PooledDynamicDataView2D(IBoundedDataViewPool<T> pool)
        {
            this.tileConfiguration = pool.TileConfiguration;
            this.pool = pool ?? throw new ArgumentNullException(nameof(pool));
            this.index = new Dictionary<TileIndex, IPooledBoundedDataView<T>>();
            this.expired = new List<(TileIndex idx, IPooledBoundedDataView<T> view)>();
        }

        public int OffsetX => tileConfiguration.OffsetX;
        public int OffsetY => tileConfiguration.OffsetY;
        public int TileSizeX => tileConfiguration.TileSizeX;
        public int TileSizeY => tileConfiguration.TileSizeY;

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

        public void ExpireAll()
        {
            expired.Clear();

            try
            {
                foreach (var entry in index)
                {
                    var e = entry.Value;
                    e.CommitUseTimePeriod();
                    ViewChunkExpired?.Invoke(this, new DynamicDataView2DEventArgs<T>(entry.Key, e));
                    expired.Add((entry.Key, entry.Value));
                }

                if (expired.Count != 0)
                {
                    activeBounds = default;
                    foreach (var e in expired)
                    {
                        index.Remove(e.idx);
                        pool.Return(e.view);
                    }
                }
            }
            finally
            {
                expired.Clear();
            }
        }

        public BufferList<Rectangle> GetActiveTiles(BufferList<Rectangle>? data = null)
        {
            data = BufferList.PrepareBuffer(data);

            foreach (var k in index.Values)
            {
                data.Add(k.Bounds);
            }

            return data;
        }

        public BufferList<Rectangle> GetDirtyTiles(BufferList<Rectangle>? data = null)
        {
            data = BufferList.PrepareBuffer(data);

            foreach (var k in index.Values)
            {
                if (k.IsUsedForWriting)
                {
                    data.Add(k.Bounds);
                }
            }

            return data;
        }

        public void PrepareFrame(long time)
        {
            currentTime = time;
            foreach (var e in index.Values)
            {
                e.BeginUseTimePeriod(time);
            }
        }

        public void Clear()
        {
            foreach (var e in index.Values)
            {
                e.MarkUsedForWriting();
                e.Clear();
            }
        }

        public void Fill(in T value)
        {
            foreach (var e in index.Values)
            {
                e.MarkUsedForWriting();
                e.Fill(in value);
            }
        }

        public bool RemoveView(int x, int y, out TileIndex removedIndex)
        {
            var idx = tileConfiguration.TileIndex(x, y);
            if (!index.TryGetValue(idx, out var data))
            {
                removedIndex = default;
                return false;
            }

            ViewChunkExpired?.Invoke(this, new DynamicDataView2DEventArgs<T>(idx, data));
            index.Remove(idx);
            pool.Return(data);
            removedIndex = idx;
            return true;
        }

        public void ExpireFrames(long age)
        {
            expired.Clear();
            try
            {
                foreach (var entry in index)
                {
                    var e = entry.Value;
                    e.CommitUseTimePeriod();
                    if ((currentTime - e.LastUsed) >= age)
                    {
                        ViewChunkExpired?.Invoke(this, new DynamicDataView2DEventArgs<T>(entry.Key, e));
                        expired.Add((entry.Key, entry.Value));
                    }
                }

                if (expired.Count != 0)
                {
                    activeBounds = default;
                    foreach (var e in expired)
                    {
                        index.Remove(e.idx);
                        pool.Return(e.view);
                    }
                }
            }
            finally
            {
                expired.Clear();
            }
        }

        public IPooledBoundedDataView<T> GetOrCreateData(int x, int y)
        {
            if (TryGetDataInternal(x, y, out IPooledBoundedDataView<T> rawData))
            {
                rawData.MarkUsedForWriting();
                return rawData;
            }

            var (idx, bounds) = tileConfiguration.Configure(x, y);
            var data = pool.Lease(bounds, currentTime);
            data.MarkUsedForReading();
            data.MarkUsedForWriting();

            index[idx] = data;
            ViewChunkCreated?.Invoke(this, new DynamicDataView2DEventArgs<T>(idx, data));
            activeBounds = default;
            return data;
        }

        public bool TryGetData(int x, int y, [MaybeNullWhen(false)] out IReadOnlyBoundedDataView<T> raw)
        {
            if (TryGetDataInternal(x, y, out IPooledBoundedDataView<T> t))
            {
                raw = t;
                return true;
            }

            raw = default;
            return false;
        }

        public bool TryGetWriteAccess(int x, int y, [MaybeNullWhen(false)] out IBoundedDataView<T> raw, DataViewCreateMode mode = DataViewCreateMode.Nothing)
        {
            if (TryGetDataInternal(x, y, out IPooledBoundedDataView<T> t))
            {
                raw = t;
                t.MarkUsedForReading();
                t.MarkUsedForWriting();
                return true;
            }

            if (mode == DataViewCreateMode.Nothing)
            {
                raw = default;
                return false;
            }

            var (idx, bounds) = tileConfiguration.Configure(x, y);
            var data = pool.Lease(bounds, currentTime);
            data.MarkUsedForReading();
            data.MarkUsedForWriting();

            index[idx] = data;
            ViewChunkCreated?.Invoke(this, new DynamicDataView2DEventArgs<T>(idx, data));
            activeBounds = default;
            raw = data;
            return true;
        }

        public bool TryGetRawAccess(int x, int y, [MaybeNullWhen(false)] out IBoundedDataViewRawAccess<T> raw)
        {
            if (TryGetDataInternal(x, y, out IPooledBoundedDataView<T> t))
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

        bool TryGetDataInternal(int x, int y, out IPooledBoundedDataView<T> data)
        {
            var idx = tileConfiguration.TileIndex(x, y);
            if (!index.TryGetValue(idx, out data))
            {
                return false;
            }

            data.MarkUsedForReading();
            return true;
        }

        public bool TryGet(int x, int y, [MaybeNullWhen(false)] out T result)
        {
            if (TryGetDataInternal(x, y, out IPooledBoundedDataView<T> data))
            {
                return data.TryGet(x, y, out result);
            }

            result = default;
            return false;
        }

        public ref T? TryGetForUpdate(int x, int y, ref T? defaultValue, out bool success, DataViewCreateMode mode = DataViewCreateMode.Nothing)
        {
            if (TryGetWriteAccess(x, y, out var data, mode))
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

                return default!;
            }
            set
            {
                TrySet(x, y, in value);
            }
        }

        [IgnoreDataMember]
        [IgnoreMember]
        public T this[GridPosition2D pos]
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
