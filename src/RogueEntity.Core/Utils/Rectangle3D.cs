using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Utils
{
    [Serializable]
    [DataContract]
    public readonly struct Rectangle3D : IEquatable<Rectangle3D>,
                                         IEquatable<(int x, int y, int z, int width, int height, int depth)>
    {
        [NonSerialized]
        public static readonly Rectangle Empty = new Rectangle(0, 0, 0, 0);

        /// <summary>
        /// The width of the rectangle.
        /// </summary>
        [DataMember]
        public readonly int Width;

        /// <summary>
        /// The height of the rectangle (y-axis).
        /// </summary>
        [DataMember]
        public readonly int Height;

        /// <summary>
        /// The depth of the rectangle (z-axis).
        /// </summary>
        [DataMember]
        public readonly int Depth;

        /// <summary>
        /// X-coordinate of position of the rectangle.
        /// </summary>
        [DataMember]
        public readonly int X;

        /// <summary>
        /// Y-coordinate of position of the rectangle.
        /// </summary>
        [DataMember]
        public readonly int Y;

        /// <summary>
        /// Z-coordinate of position of the rectangle.
        /// </summary>
        [DataMember]
        public readonly int Z;

        public Rectangle3D(int x, int y, int z, int width, int height, int depth)
        {
            Width = width;
            Height = height;
            Depth = depth;
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(Rectangle3D other)
        {
            return Width == other.Width && Height == other.Height && Depth == other.Depth && X == other.X && Y == other.Y && Z == other.Z;
        }

        public bool Equals((int x, int y, int z, int width, int height, int depth) other)
        {
            return Width == other.width && Height == other.height && Depth == other.depth && X == other.x && Y == other.y && Z == other.z;
        }

        public override bool Equals(object obj)
        {
            return obj is Rectangle3D other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Width;
                hashCode = (hashCode * 397) ^ Height;
                hashCode = (hashCode * 397) ^ Depth;
                hashCode = (hashCode * 397) ^ X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Z;
                return hashCode;
            }
        }

        public static bool operator ==(Rectangle3D left, Rectangle3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Rectangle3D left, Rectangle3D right)
        {
            return !left.Equals(right);
        }

        public Rectangle ToLayerSlice() => new Rectangle(X, Y, Width, Height);

        public override string ToString()
        {
            return $"Rectangle3D({nameof(X)}: {X}, {nameof(Y)}: {Y}, {nameof(Z)}: {Z}, {nameof(Width)}: {Width}, {nameof(Height)}: {Height}, {nameof(Depth)}: {Depth})";
        }

        public void Deconstruct(out int width, out int height, out int depth, out int x, out int y, out int z)
        {
            width = Width;
            height = Height;
            depth = Depth;
            x = X;
            y = Y;
            z = Z;
        }

        public void Deconstruct(out int width, out int height, out int x, out int y)
        {
            width = Width;
            height = Height;
            x = X;
            y = Y;
        }

        public RangeEnumerable Layers => new RangeEnumerable(Math.Min(Z, Z + Depth), Math.Max(Z, Z + Depth));

        public bool Contains(double x, double y, double z)
        {
            return x >= X && y >= Y && z >= Z &&
                   x < X + Width && y < Y + Height && z < Z + Depth;
        }
    }

    public readonly struct RangeEnumerable : IEnumerable<int>
    {
        readonly int start;
        readonly int end;

        public RangeEnumerable(int start, int end)
        {
            this.start = start;
            this.end = end;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            return new RangeEnumerator(start, end);
        }

        public RangeEnumerator GetEnumerator()
        {
            return new RangeEnumerator(start, end);
        }
    }

    public struct RangeEnumerator : IEnumerator<int>
    {
        readonly int start;
        readonly int end;
        int current;

        public RangeEnumerator(int start, int end)
        {
            if (end < start) throw new ArgumentOutOfRangeException();

            this.start = start;
            this.end = end;
            this.current = start;
        }

        public void Dispose()
        { }

        public bool MoveNext()
        {
            if (current + 1 < end)
            {
                current += 1;
                return true;
            }

            current = default;
            return false;
        }

        public void Reset()
        {
            current = start;
        }

        object IEnumerator.Current => Current;

        public int Current
        {
            get
            {
                return current;
            }
        }
    }
}
