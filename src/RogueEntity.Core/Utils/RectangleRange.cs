using System.Collections;
using System.Collections.Generic;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Utils
{
    public readonly struct RectangleRange : IEnumerable<Position2D>
    {
        readonly Rectangle r;

        public RectangleRange(int w, int h)
        {
            r = new Rectangle(0, 0, w, h);
        }

        public RectangleRange(Rectangle r)
        {
            this.r = r;
        }

        public RectangleRangeEnumerator GetEnumerator()
        {
            return new RectangleRangeEnumerator(r);
        }

        IEnumerator<Position2D> IEnumerable<Position2D>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static RectangleRange Of(int width, int height)
        {
            return new RectangleRange(width, height);
        }

        public static RectangleRange Of(int x, int y, int width, int height)
        {
            return new RectangleRange(new Rectangle(x, y, width, height));
        }
    }
}