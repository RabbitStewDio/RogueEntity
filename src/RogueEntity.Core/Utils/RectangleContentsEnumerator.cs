using System.Collections;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils
{
    public struct RectangleContentsEnumerator : IEnumerator<GridPosition2D>
    {
        readonly GridPosition2D origin;
        readonly int width;
        readonly int maxIdx;
        int idx;

        public RectangleContentsEnumerator(Rectangle r) : this(r.X, r.Y, r.Width, r.Height)
        {
        }

        public RectangleContentsEnumerator(int x, int y, int width, int height)
        {
            this.origin = new GridPosition2D(x, y);
            this.width = width;
            maxIdx = (width * height) - 1;
            idx = -1;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (idx >= maxIdx) return false;
            idx += 1;
            return true;
        }

        public void Reset()
        {
            idx = -1;
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public GridPosition2D Current
        {
            get
            {
                return GridPosition2D.FromLinearIndex(idx, width) + origin;
            }
        }
    }
}