using System;
using System.Text;

namespace RogueEntity.Core.Utils.Maps
{
    public class PackedBoolMap : IMapData<bool>
    {
        public int Width { get; }
        public int Height { get; }
        readonly uint[] data;

        public PackedBoolMap(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException();
            if (height <= 0) throw new ArgumentOutOfRangeException();

            var size = Align(width * height, 32);
            data = new uint[size];
            Width = width;
            Height = height;
        }

        public bool this[int x, int y]
        {
            get
            {
                var chunkIndex = Math.DivRem(y * Width + x, 32, out var innerIndex);
                var chunk = data[chunkIndex];
                if (chunk == 0) return false;
                if (chunk == uint.MaxValue) return true;

                var bitMask = 1u << innerIndex;
                return (chunk & bitMask) == bitMask;
            }
            set
            {
                var chunkIndex = Math.DivRem(y * Width + x, 32, out var innerIndex);
                var bitMask = (1u << innerIndex);
                if (value)
                {
                    data[chunkIndex] |= bitMask;
                }
                else
                {
                    data[chunkIndex] &= ~bitMask;
                }
            }
        }

        static int Align(int v, int boundary)
        {
            return (int)Math.Ceiling(v / (float)boundary) * boundary;
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            for (int y = 0; y < Height; y += 1)
            {
                for (int x = 0; x < Width; x += 1)
                {
                    b.Append(this[x, y] ? '*' : '.');
                }

                b.Append('\n');
            }

            return b.ToString();
        }

        public void Clear()
        {
            Array.Clear(data, 0, data.Length);
        }
    }
}