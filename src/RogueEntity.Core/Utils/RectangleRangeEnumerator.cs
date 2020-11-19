using System.Collections;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils
{
    public struct RectangleRangeEnumerator : IEnumerator<Position2D>
    {
        readonly int minX;
        readonly int maxX;
        readonly int minY;
        readonly int maxY;
        int side;
        int pos;

        public RectangleRangeEnumerator(Rectangle r)
        {
            this.minX = r.MinExtentX;
            this.minY = r.MinExtentY;
            this.maxX = r.MaxExtentX;
            this.maxY = r.MaxExtentY;
            side = 0;
            pos = -1;
            Current = default;
        }

        public bool MoveNext()
        {
            if (minX == maxX && minY == maxY)
            {
                if (pos == -1)
                {
                    pos = 0;
                    Current = new Position2D(minX, minY);
                    return true;
                }

                Current = default;
                return false;
            }

            while (ProduceIterator(out var incr, out var start, out _, out int maxPos))
            {
                pos += 1;
                if (pos < maxPos)
                {
                    Current = start + incr * pos;
                    return true;
                }

                pos = -1;
                side += 1;
                if (side == 2 && minX == maxX)
                {
                    side = 3;
                }

                if (side == 3 && minY == maxY)
                {
                    Current = default;
                    return false;
                }
            }

            Current = default;
            return false;
            
        }

        bool ProduceIterator(out Position2D increment, out Position2D start, out Position2D end, out int maxPos)
        {
            switch (side)
            {
                case 0:
                    increment = new Position2D(1, 0);
                    start = new Position2D(minX, minY);
                    end = new Position2D(maxX, minY);
                    maxPos = maxX - minX;
                    return true;
                case 1:
                    increment = new Position2D(0, 1);
                    start = new Position2D(maxX, minY);
                    end = new Position2D(maxX, maxY);
                    maxPos = maxY - minY;
                    return true;
                case 2:
                    increment = new Position2D(-1, 0);
                    start = new Position2D(maxX, maxY);
                    end = new Position2D(minX, maxY);
                    maxPos = maxX - minX;
                    return true;
                case 3:
                    increment = new Position2D(0, -1);
                    start = new Position2D(minX, maxY);
                    end = new Position2D(minX, minY);
                    maxPos = maxY - minY;
                    return true;
                default:
                    increment = default;
                    start = default;
                    end = default;
                    maxPos = 0;
                    return false;
            }
        }

        public void Dispose()
        {
        }

        public void Reset()
        {
            side = 0;
            pos = -1;
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public Position2D Current { get; private set; }
    }
}