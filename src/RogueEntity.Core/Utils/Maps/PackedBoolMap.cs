using System;
using System.Runtime.Serialization;
using System.Text;
using MessagePack;

namespace RogueEntity.Core.Utils.Maps
{
    [Serializable]
    [DataContract]
    [MessagePackObject]
    public class PackedBoolMap : IMapData<bool>, IEquatable<PackedBoolMap>
    {
        [Key(0)]
        [DataMember(Order = 0)]
        readonly int width;
        [Key(1)]
        [DataMember(Order = 0)]
        readonly int height;

        [DataMember(Order = 2)]
        [Key(2)]
        readonly byte[] data;

        public PackedBoolMap(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            var size = Align(width * height, WordSize);
            data = new byte[size];
            this.width = width;
            this.height = height;
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public int Width => width;

        [IgnoreMember]
        [IgnoreDataMember]
        public int Height => height;

        public const int WordSize = 8;

        public bool Any(int x, int y)
        {
            var chunkIndex = Math.DivRem(y * Width + x, WordSize, out _);
            var chunk = data[chunkIndex];
            return chunk != 0;
        }
        
        public bool this[int x, int y]
        {
            get
            {
                var chunkIndex = Math.DivRem(y * Width + x, WordSize, out var innerIndex);
                var chunk = data[chunkIndex];
                if (chunk == 0) return false;
                if (chunk == byte.MaxValue) return true;

                var bitMask = 1u << innerIndex;
                return (chunk & bitMask) == bitMask;
            }
            set
            {
                var chunkIndex = Math.DivRem(y * Width + x, WordSize, out var innerIndex);
                var bitMask = (byte) (1u << innerIndex);
                if (value)
                {
                    data[chunkIndex] |= bitMask;
                }
                else
                {
                    var bdata = data[chunkIndex] & ~bitMask;
                    data[chunkIndex] = (byte) bdata;
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

        public bool Equals(PackedBoolMap other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return CoreExtensions.EqualsList(data, other.data) && Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((PackedBoolMap) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (data != null ? data.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Width;
                hashCode = (hashCode * 397) ^ Height;
                return hashCode;
            }
        }

        public static bool operator ==(PackedBoolMap left, PackedBoolMap right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PackedBoolMap left, PackedBoolMap right)
        {
            return !Equals(left, right);
        }
    }
}