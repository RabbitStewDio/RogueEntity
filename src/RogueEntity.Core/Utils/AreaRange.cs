using System;
using System.Collections;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils
{
    public struct AreaRange: IEnumerable<Position2D>
    {
        readonly Rectangle r;

        public AreaRange(int w, int h)
        {
            r = new Rectangle(0, 0, w, h);
        }

        public AreaRange(Rectangle r)
        {
            this.r = r;
        }

        public struct Enumerator: IEnumerator<Position2D>
        {
            readonly Rectangle contents;
            int index;

            internal Enumerator(Rectangle widget) : this()
            {
                this.contents = widget;
                index = -1;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (index + 1 < (contents.Width * contents.Height))
                {
                    index += 1;
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                index = -1;
            }

            object IEnumerator.Current => Current;

            public Position2D Current
            {
                get
                {
                    if (index < 0 || index >= (contents.Width * contents.Height))
                    {
                        throw new InvalidOperationException();
                    }

                    var x = index % contents.Width;
                    var y = index / contents.Width;
                    return new Position2D(contents.X + x, contents.Y + y);
                }
            }

        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(r);
        }

        IEnumerator<Position2D> IEnumerable<Position2D>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static AreaRange Of(int width, int height)
        {
            return new AreaRange(width, height);
        }

        public static AreaRange Of(int x, int y, int width, int height)
        {
            return new AreaRange(new Rectangle( x, y, width, height));
        }
    }
}