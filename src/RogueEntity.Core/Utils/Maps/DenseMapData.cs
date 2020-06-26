using System;

namespace RogueEntity.Core.Utils.Maps
{
    public class DenseMapData<T> : IMapData<T>
    {
        readonly T[,] cells;

        public DenseMapData(int width, int height)
        {
            if (width <= 0) throw new ArgumentException();
            if (height <= 0) throw new ArgumentException();

            Width = width;
            Height = height;
            cells = new T[width, height];
        }

        public int Width { get; }
        public int Height { get; }

        public T this[int x, int y]
        {
            get { return cells[x, y]; }
            set { cells[x, y] = value; }
        }

        public void Clear()
        {
            Array.Clear(cells, 0, cells.Length);
        }

        public void CopyFrom(DenseMapData<T> other)
        {
            if (other.Width != Width || other.Height != Height)
            {
                throw new ArgumentException("Can only copy maps of the same configuration.");
            }

            Array.Copy(other.cells, cells, cells.Length);
        }
    }
}