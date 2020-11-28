using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Utils.DataViews
{
    /// <summary>
    ///   A data view backed up by a dense array, but able to handle 
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    [DataContract]
    [MessagePackObject]
    public class BoundedDataView<TData> : IBoundedDataViewRawAccess<TData>, IEquatable<BoundedDataView<TData>>
    {
        [DataMember(Order = 0)]
        [Key(0)]
        Rectangle bounds;

        [DataMember(Order = 1)]
        [Key(1)]
        TData[] data;

        public BoundedDataView(in Rectangle bounds)
        {
            this.bounds = bounds;
            this.data = new TData[bounds.Width * bounds.Height];
        }

        [SerializationConstructor]
        public BoundedDataView(Rectangle bounds, TData[] data)
        {
            if (data.Length < bounds.Width * bounds.Height)
            {
                throw new ArgumentException();
            }

            this.bounds = bounds;
            this.data = data;
        }

        [IgnoreDataMember]
        [IgnoreMember]
        public Rectangle Bounds => bounds;

        [IgnoreDataMember]
        [IgnoreMember]
        public TData[] Data => data;

        public IReadOnlyList<TData> RawData => Data;

        public void Resize(in Rectangle newBounds, bool strict = false)
        {
            if (bounds.Width == newBounds.Width &&
                bounds.Height == newBounds.Height)
            {
                this.bounds = newBounds;
                return;
            }

            if (!strict &&
                (bounds.Width >= newBounds.Width &&
                 bounds.Height >= newBounds.Height))
            {
                // rebase origin, but dont change coordinates.
                this.bounds = new Rectangle(newBounds.MinExtentX, newBounds.MinExtentY, bounds.Width, bounds.Height);
                Array.Clear(data, 0, data.Length);
            }
            else
            {
                this.bounds = newBounds;
                this.data = new TData[bounds.Width * bounds.Height];
            }
        }

        public void Clear()
        {
            Array.Clear(data, 0, data.Length);
        }

        public void Clear(in Rectangle clearBounds)
        {
            var boundsLocal = this.bounds.GetIntersection(clearBounds);
            if (boundsLocal.Width == 0 || boundsLocal.Height == 0)
            {
                return;
            }

            var origin = this.bounds.Position;
            var minY = boundsLocal.Y - origin.Y;
            var maxY = minY + boundsLocal.Height;

            for (int y = minY; y < maxY; y += 1)
            {
                var idx = clearBounds.Width * y;
                Array.Clear(data, idx, boundsLocal.Width);
            }
        }

        public void Fill(in TData v)
        {
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = v;
            }
        }
        
        public void Fill(in Rectangle fillBounds, in TData v)
        {
            var boundsLocal = this.bounds.GetIntersection(fillBounds);
            if (boundsLocal.Width == 0 || boundsLocal.Height == 0)
            {
                return;
            }

            var origin = this.bounds.Position;
            var minY = boundsLocal.Y - origin.Y;
            var maxY = minY + boundsLocal.Height;

            for (int y = minY; y < maxY; y += 1)
            {
                var idx = fillBounds.Width * y;
                var idxMax = idx + boundsLocal.Width;
                for (int x = idx; x < idxMax; x += 1)
                {
                    data[x] = v;
                }
            }
        }

        public bool TryGetRawIndex<TPosition2D>(in TPosition2D pos, out int result)
            where TPosition2D : IPosition2D<TPosition2D>
            => TryGetRawIndex(pos.X, pos.Y, out result);

        public bool TryGetRawIndex(int posX, int posY, out int result)
        {
            if (bounds.Contains(posX, posY))
            {
                var rawX = posX - bounds.X;
                var rawY = posY - bounds.Y;
                result = rawX + rawY * bounds.Width;
                return true;
            }

            result = default;
            return false;
        }

        public bool TryGet(int x, int y, out TData result)
        {
            var rawX = x - bounds.X;
            var rawY = y - bounds.Y;
            if (rawX < 0 || rawY < 0 ||
                rawX >= bounds.Width || 
                rawY >= bounds.Height)
            {
                result = default;
                return false;
            }

            var linIdx = rawX + rawY * bounds.Width;
            result = data[linIdx];
            return true;
        }

        public bool TryGet<TPosition2D>(in TPosition2D pos, out TData result)
            where TPosition2D : IPosition2D<TPosition2D>
        {
            return TryGet(pos.X, pos.Y, out result);
        }

        public bool TrySet(int x, int y, in TData result)
        {
            var rawX = x - bounds.X;
            var rawY = y - bounds.Y;
            if (rawX < 0 || rawY < 0 ||
                rawX >= bounds.Width || rawY >= bounds.Height)
            {
                return false;
            }

            var linIdx = rawX + rawY * bounds.Width;
            data[linIdx] = result;
            return true;
        }

        public bool TrySet<TPosition2D>(in TPosition2D pos, in TData result)
            where TPosition2D : IPosition2D<TPosition2D>
        {
            if (bounds.Contains(pos.X, pos.Y))
            {
                var rawX = pos.X - bounds.X;
                var rawY = pos.Y - bounds.Y;
                var linIdx = rawX + rawY * bounds.Width;
                data[linIdx] = result;
                return true;
            }

            return false;
        }

        [IgnoreDataMember]
        [IgnoreMember]
        public TData this[int x, int y]
        {
            get
            {
                if (!TryGet(x, y, out var result))
                {
                    throw new ArgumentOutOfRangeException();
                }

                return result;
            }
            set
            {
                if (!TrySet(x, y, in value))
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        [IgnoreDataMember]
        [IgnoreMember]
        public TData this[in Position2D pos]
        {
            get
            {
                if (!TryGet(pos, out var result))
                {
                    throw new ArgumentOutOfRangeException();
                }

                return result;
            }
            set
            {
                if (!TrySet(pos, in value))
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool TryGetFromRawIndex(int idx, out Position2D pos)
        {
            if (idx < 0 || idx >= data.Length)
            {
                pos = default;
                return false;
            }

            var y = Math.DivRem(idx, bounds.Width, out var x);
            pos = new Position2D(x + bounds.X, y + bounds.Y);
            return true;
        }

        public bool Equals(BoundedDataView<TData> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return bounds.Equals(other.bounds) && CoreExtensions.EqualsList(data, other.data);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((BoundedDataView<TData>)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return bounds.GetHashCode();
        }

        public static bool operator ==(BoundedDataView<TData> left, BoundedDataView<TData> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BoundedDataView<TData> left, BoundedDataView<TData> right)
        {
            return !Equals(left, right);
        }
    }
}