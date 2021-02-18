using System.Collections;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils
{
    public readonly struct RectangleContents : IEnumerable<Position2D>
    {
        readonly Rectangle data;

        public RectangleContents(Rectangle data)
        {
            this.data = data;
        }

        public RectangleContents(int x, int y, int width, int height)
        {
            this.data = new Rectangle(x, y, width, height);
        }

        public RectangleContents(int width, int height)
        {
            this.data = new Rectangle(0, 0, width, height);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<Position2D> IEnumerable<Position2D>.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public RectangleContentsEnumerator GetEnumerator() => new RectangleContentsEnumerator(data.X, data.Y, data.Width, data.Height);
    }
}
