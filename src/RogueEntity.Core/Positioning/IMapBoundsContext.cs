using System;

namespace RogueEntity.Core.Positioning
{
    public interface IMapBoundsContext
    {
        MapBoundary MapExtent { get; }
    }

    public readonly struct MapBoundary : IEquatable<MapBoundary>
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int Depth;

        public MapBoundary(int width, int height, int depth)
        {
            Width = width;
            Height = height;
            Depth = depth;
        }

        public bool Equals(MapBoundary other)
        {
            return Width == other.Width && Height == other.Height && Depth == other.Depth;
        }

        public override bool Equals(object obj)
        {
            return obj is MapBoundary other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Width;
                hashCode = (hashCode * 397) ^ Height;
                hashCode = (hashCode * 397) ^ Depth;
                return hashCode;
            }
        }

        public static bool operator ==(MapBoundary left, MapBoundary right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MapBoundary left, MapBoundary right)
        {
            return !left.Equals(right);
        }
    }
}