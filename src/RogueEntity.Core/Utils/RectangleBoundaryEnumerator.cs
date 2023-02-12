using System.Collections;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils
{
    public struct RectangleBoundaryEnumerator : IEnumerator<GridPosition2D>
    {
        readonly int minX;
        readonly int maxX;
        readonly int minY;
        readonly int maxY;
        int side;
        int pos;

        public RectangleBoundaryEnumerator(Rectangle r)
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
                    Current = new GridPosition2D(minX, minY);
                    return true;
                }

                Current = default;
                return false;
            }

            while (ProduceIterator(out var incr, out var start, out int maxPos))
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

        bool ProduceIterator(out GridPosition2D increment, out GridPosition2D start, out int maxPos)
        {
            switch (side)
            {
                case 0:
                    increment = new GridPosition2D(1, 0);
                    start = new GridPosition2D(minX, minY);
                    maxPos = maxX - minX;
                    return true;
                case 1:
                    increment = new GridPosition2D(0, 1);
                    start = new GridPosition2D(maxX, minY);
                    maxPos = maxY - minY;
                    return true;
                case 2:
                    increment = new GridPosition2D(-1, 0);
                    start = new GridPosition2D(maxX, maxY);
                    maxPos = maxX - minX;
                    return true;
                case 3:
                    increment = new GridPosition2D(0, -1);
                    start = new GridPosition2D(minX, maxY);
                    maxPos = maxY - minY;
                    return true;
                default:
                    increment = default;
                    start = default;
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

        public GridPosition2D Current { get; private set; }
    }
}