using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using MessagePack;
using RogueEntity.Api.Utils;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Utils.DataViews
{
    [DataContract]
    [MessagePackObject]
    public class DynamicBoolDataView2D : IDynamicDataView2D<bool>
    {
        public event EventHandler<DynamicDataView2DEventArgs<bool>>? ViewChunkCreated;
        public event EventHandler<DynamicDataView2DEventArgs<bool>>? ViewChunkExpired;

        [DataMember(Order = 0)]
        [Key(0)]
        readonly int tileSizeX;

        [DataMember(Order = 1)]
        [Key(1)]
        readonly int tileSizeY;

        [DataMember(Order = 2)]
        [Key(2)]
        readonly Dictionary<TileIndex, TrackedDataView> index;

        [DataMember(Order = 3)]
        [Key(3)]
        long currentTime;

        [DataMember(Order = 4)]
        [Key(4)]
        int offsetX;

        [DataMember(Order = 5)]
        [Key(5)]
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
        readonly List<TileIndex> expired;

        [IgnoreDataMember]
        [IgnoreMember]
        Rectangle activeBounds;

        public DynamicBoolDataView2D(int tileSizeX, int tileSizeY) : this(0, 0, tileSizeX, tileSizeY)
        {
        }

        public DynamicBoolDataView2D(int offsetX, int offsetY, int tileSizeX, int tileSizeY)
        {
            if (tileSizeX <= 0) throw new ArgumentException(nameof(tileSizeX));
            if (tileSizeY <= 0) throw new ArgumentException(nameof(tileSizeY));

            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.tileSizeX = tileSizeX;
            this.tileSizeY = tileSizeY;
            index = new Dictionary<TileIndex, TrackedDataView>();
            expired = new List<TileIndex>();
        }

        [SerializationConstructor]
        internal DynamicBoolDataView2D(int tileSizeX,
                                     int tileSizeY,
                                     Dictionary<TileIndex, TrackedDataView> index,
                                     long currentTime,
                                     int offsetX,
                                     int offsetY)
        {
            this.tileSizeX = tileSizeX;
            this.tileSizeY = tileSizeY;
            this.index = index ?? throw new ArgumentNullException(nameof(index));
            this.expired = new List<TileIndex>();
            this.currentTime = currentTime;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            foreach (var idx in index.Values)
            {
                idx.MarkUnused(currentTime);
            }
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

        public BufferList<Rectangle> GetActiveTiles(BufferList<Rectangle>? data = null)
        {
            data = BufferList.PrepareBuffer(data);

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

        public void Clear()
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

        public void Fill(in bool value)
        {
            foreach (var e in index.Values)
            {
                e.MarkUsedForWriting();
                e.Fill(in value);
            }
        }

        public bool RemoveView(int x, int y, out TileIndex removedIndex)
        {
            var dx = DataViewPartitions.TileSplit(x, offsetX, tileSizeX);
            var dy = DataViewPartitions.TileSplit(y, offsetY, tileSizeY);

            var key = new TileIndex(dx, dy);
            if (index.TryGetValue(key, out var existing))
            {
                ViewChunkExpired?.Invoke(this, new DynamicDataView2DEventArgs<bool>(key, existing));
                index.Remove(key);
                removedIndex = key;
                return true;
            }

            removedIndex = default;
            return false;

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
                    ViewChunkExpired?.Invoke(this, new DynamicDataView2DEventArgs<bool>(entry.Key, e));
                    expired.Add(entry.Key);
                }
            }

            if (expired.Count != 0)
            {
                activeBounds = default;
                foreach (var e in expired)
                {
                    index.Remove(e);
                }
            }
        }

        public bool Any(int x, int y)
        {
            if (TryGetDataInternal(x, y, out var data))
            {
                return data.Any(x, y);
            }

            return false;
        }

        public BoundedBoolDataView GetOrCreateData(int x, int y)
        {
            var dx = DataViewPartitions.TileSplit(x, offsetX, tileSizeX);
            var dy = DataViewPartitions.TileSplit(y, offsetY, tileSizeY);
            var key = new TileIndex(dx, dy);
            if (index.TryGetValue(key, out var data))
            {
                data.MarkUsedForWriting();
                return data;
            }

            data = new TrackedDataView(new Rectangle(dx * tileSizeX + offsetX, dy * tileSizeY + offsetY, tileSizeX, tileSizeY), currentTime);
            index[key] = data;
            ViewChunkCreated?.Invoke(this, new DynamicDataView2DEventArgs<bool>(key, data));
            activeBounds = default;
            return data;

        }

        public bool TrySet(int x, int y, in bool value)
        {
            var data = GetOrCreateData(x, y);
            return data[x, y] = value;
        }

        bool IReadOnlyDynamicDataView2D<bool>.TryGetData(int x, int y, [MaybeNullWhen(false)] out IReadOnlyBoundedDataView<bool> raw)
        {
            if (TryGetData(x, y, out var data))
            {
                raw = data;
                return true;
            }

            raw = default;
            return false;
        }

        public bool TryGetData(int x, int y, [MaybeNullWhen(false)] out IReadOnlyBoundedBoolDataView data)
        {
            if (TryGetDataInternal(x, y, out var v))
            {
                data = v;
                return true;
            }

            data = default;
            return false;
        }

        public bool TryGetWriteAccess(int x, int y, [MaybeNullWhen(false)] out IBoundedDataView<bool> data, DataViewCreateMode mode = DataViewCreateMode.Nothing)
        {
            if (TryGetDataInternal(x, y, out var v))
            {
                data = v;
                return true;
            }

            if (mode == DataViewCreateMode.Nothing)
            {
                data = default;
                return false;
            }
            
            var dx = DataViewPartitions.TileSplit(x, offsetX, tileSizeX);
            var dy = DataViewPartitions.TileSplit(y, offsetY, tileSizeY);
            var dataRaw = new TrackedDataView(new Rectangle(dx * tileSizeX + offsetX, dy * tileSizeY + offsetY, tileSizeX, tileSizeY), currentTime);
            var key = new TileIndex(dx, dy);
            index[key] = dataRaw;
            ViewChunkCreated?.Invoke(this, new DynamicDataView2DEventArgs<bool>(key, dataRaw));
            activeBounds = default;
            
            data = dataRaw;
            return true;
        }

        public ref bool TryGetForUpdate(int x, int y, ref bool defaultValue, out bool success, DataViewCreateMode mode = DataViewCreateMode.Nothing)
        {
            throw new InvalidOperationException();
        }

        public bool TryGetRawAccess(int x, int y, [MaybeNullWhen(false)] out IBoundedDataViewRawAccess<bool> raw)
        {
            raw = default;
            return false;
        }

        bool TryGetDataInternal(int x, int y, [MaybeNullWhen(false)] out TrackedDataView data)
        {
            var dx = DataViewPartitions.TileSplit(x, offsetX, tileSizeX);
            var dy = DataViewPartitions.TileSplit(y, offsetY, tileSizeY);
            if (!index.TryGetValue(new TileIndex(dx, dy), out data))
            {
                return false;
            }

            data.MarkUsedForReading();
            return true;
        }

        public bool TryGet(int x, int y, out bool result)
        {
            if (TryGetDataInternal(x, y, out var data))
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

        public bool AnyValueSetInTile(int x, int y)
        {
            if (!TryGetDataInternal(x, y, out var tile))
            {
                return false;
            }

            return tile.AnyValueSet();
        }

        [DataContract]
        [MessagePackObject]
        internal class TrackedDataView : BoundedBoolDataView
        {
            [IgnoreMember]
            [IgnoreDataMember]
            long currentTime;

            [IgnoreMember]
            [IgnoreDataMember]
            public long LastUsed { get; private set; }

            [IgnoreMember]
            [IgnoreDataMember]
            int usedForReading;

            [IgnoreMember]
            [IgnoreDataMember]
            int usedForWriting;

            [SerializationConstructor]
            protected internal TrackedDataView(Rectangle bounds, byte[] data, int anyValueSet) : base(bounds, data, anyValueSet)
            {
            }

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

            [IgnoreMember]
            [IgnoreDataMember]
            public bool IsUsedForReading => usedForReading != 0;

            [IgnoreMember]
            [IgnoreDataMember]
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