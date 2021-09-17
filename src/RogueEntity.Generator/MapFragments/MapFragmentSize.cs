using JetBrains.Annotations;
using System;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Generator.MapFragments
{
    /// <summary>
    ///   Only used during parsing. 
    /// </summary>
    [UsedImplicitly]
    struct MapFragmentSize : IEquatable<MapFragmentSize>
    {
        public int Width { get; set; }
        public int Height { get; set; }

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

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
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
