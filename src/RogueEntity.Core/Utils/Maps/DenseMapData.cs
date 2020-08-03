using System;
using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Utils.Maps
{
    [Serializable]
    [DataContract]
    [MessagePackObject]
    public class DenseMapData<T> : IMapData<T>
    {
        [DataMember(Order=2)]
        [Key(2)]
        readonly T[] cells;

        public DenseMapData(int width, int height)
        {
            if (width <= 0) throw new ArgumentException();
            if (height <= 0) throw new ArgumentException();

            Width = width;
            Height = height;
            cells = new T[width * height];
        }

        [Key(0)]
        [DataMember(Order = 0)]
        public int Width { get; }
        [Key(1)]
        [DataMember(Order = 1)]
        public int Height { get; }

        public T this[int x, int y]
        {
            get { return cells[x + y * Width]; }
            set { cells[x + y * Width] = value; }
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