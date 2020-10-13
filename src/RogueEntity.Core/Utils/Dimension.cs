using System;

namespace RogueEntity.Core.Utils
{
    public readonly struct Dimension : IEquatable<Dimension>
    {
        public readonly int Width;
        public readonly int Height;

        public Dimension(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public void Deconstruct(out int width, out int height)
        {
            width = Width;
            height = Height;
        }

        public bool Equals(Dimension other)
        {
            return Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            return obj is Dimension other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Width * 397) ^ Height;
            }
        }

        public static bool operator ==(Dimension left, Dimension right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Dimension left, Dimension right)
        {
            return !left.Equals(right);
        }
    }
}