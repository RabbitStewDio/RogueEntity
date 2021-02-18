using System.Collections;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils
{
    public readonly struct RectangleBoundary : IEnumerable<Position2D>
    {
        readonly Rectangle r;

        public RectangleBoundary(int w, int h)
        {
            r = new Rectangle(0, 0, w, h);
        }
        public RectangleBoundary(int x, int y, int w, int h)
        {
            r = new Rectangle(x, y, w, h);
        }

        public RectangleBoundary(Rectangle r)
        {
            this.r = r;
        }

        public RectangleBoundaryEnumerator GetEnumerator()
        {
            return new RectangleBoundaryEnumerator(r);
        }

        IEnumerator<Position2D> IEnumerable<Position2D>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static RectangleBoundary Of(int width, int height)
        {
            return new RectangleBoundary(width, height);
        }

        public static RectangleBoundary Of(int x, int y, int width, int height)
        {
            return new RectangleBoundary(new Rectangle(x, y, width, height));
        }
    }
}