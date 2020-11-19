using System.Collections;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils
{
    public struct RectangleEnumerator : IEnumerator<Position2D>
    {
        readonly Position2D origin;
        readonly int width;
        readonly int maxIdx;
        int idx;

        public RectangleEnumerator(Rectangle r) : this(r.X, r.Y, r.Width, r.Height)
        {
        }

        public RectangleEnumerator(int x, int y, int width, int height)
        {
            this.origin = new Position2D(x, y);
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

        public Position2D Current
        {
            get
            {
                return Position2D.From(idx, width) + origin;
            }
        }
    }
}