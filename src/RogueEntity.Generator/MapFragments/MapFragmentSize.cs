using JetBrains.Annotations;
using System;

namespace RogueEntity.Generator.MapFragments
{
    /// <summary>
    ///   Only used during parsing. 
    /// </summary>
    [UsedImplicitly]
    internal struct MapFragmentSize : IEquatable<MapFragmentSize>
    {
        public int Width { get; [UsedImplicitly] set; }
        public int Height { get; [UsedImplicitly] set; }

        public MapFragmentSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        internal static bool IsEmpty(MapFragmentSize s)
        {
            return s.Width == 0 || s.Height == 0;
        }

        public bool Equals(MapFragmentSize other)
        {
            return Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            return obj is MapFragmentSize other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Width * 397) ^ Height;
            }
        }

        public static bool operator ==(MapFragmentSize left, MapFragmentSize right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MapFragmentSize left, MapFragmentSize right)
        {
            return !left.Equals(right);
        }
    }
}
