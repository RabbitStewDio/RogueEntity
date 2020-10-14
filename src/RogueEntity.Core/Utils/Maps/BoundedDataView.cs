using System;
using GoRogue;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Utils.Maps
{
    /// <summary>
    ///   A data view backed up by a dense array, but able to handle 
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class BoundedDataView<TData>
    {
        Rectangle bounds;
        TData[] data;

        public BoundedDataView(in Rectangle bounds)
        {
            this.bounds = bounds;
            this.data = new TData[bounds.Width * bounds.Height];
        }

        public Rectangle Bounds => bounds;

        public TData[] Data => data;

        public void Resize(in Rectangle newBounds)
        {
            if (bounds.Width >= newBounds.Width &&
                bounds.Height >= newBounds.Height)
            {
                // rebase origin, but dont change coordinates.
                this.bounds = new Rectangle(newBounds.MinExtentX, newBounds.MinExtentY, bounds.Width, bounds.Height);
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

        public void Clear(in Rectangle bounds)
        {
            var boundsLocal = this.bounds.GetIntersection(bounds);
            if (boundsLocal.Width == 0 || boundsLocal.Height == 0)
            {
                return;
            }

            var origin = this.bounds.Position;
            var minY = boundsLocal.Y - origin.Y;
            var maxY = minY + boundsLocal.Height;

            for (int y = minY; y < maxY; y += 1)
            {
                var idx = bounds.Width * y;
                Array.Clear(data, idx, boundsLocal.Width);
            }
        }

        public bool TryGetRawIndex(in Position2D pos, out int result)
        {
            if (bounds.Contains(pos.X, pos.Y))
            {
                var rawX = pos.X - bounds.MinExtentX;
                var rawY = pos.Y - bounds.MinExtentY;
                result = rawX + rawY * bounds.Width;
                return true;
            }

            result = default;
            return false;
        }

        public bool TryGet(int x, int y, out TData result)
        {
            var rawX = x - bounds.MinExtentX;
            var rawY = y - bounds.MinExtentY;
            if (rawX < 0 || rawY < 0 ||
                rawX >= bounds.Width || rawY >= bounds.Height)
            {
                result = default;
                return false;
            }

            var linIdx = rawX + rawY * bounds.Width;
            result = data[linIdx];
            return true;
        }

        public bool TryGet(in Position2D pos, out TData result)
        {
            return TryGet(pos.X, pos.Y, out result);
        }

        public bool TrySet(int x, int y, in TData result)
        {
            var rawX = x - bounds.MinExtentX;
            var rawY = y - bounds.MinExtentY;
            if (rawX < 0 || rawY < 0 ||
                rawX >= bounds.Width || rawY >= bounds.Height)
            {
                return false;
            }

            var linIdx = rawX + rawY * bounds.Width;
            data[linIdx] = result;
            return true;
        }

        public bool TrySet(in Position2D pos, in TData result)
        {
            if (bounds.Contains(pos.X, pos.Y))
            {
                var rawX = pos.X - bounds.MinExtentX;
                var rawY = pos.Y - bounds.MinExtentY;
                var linIdx = rawX + rawY * bounds.Width;
                data[linIdx] = result;
                return true;
            }

            return false;
        }

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
    }
}