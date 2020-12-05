using System;
using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Utils.DataViews
{
    [MessagePackObject]
    [DataContract]
    public readonly struct DynamicDataViewConfiguration : IEquatable<DynamicDataViewConfiguration>
    {
        public static readonly DynamicDataViewConfiguration Default_64x64 = new DynamicDataViewConfiguration(0, 0, 64, 64);
        public static readonly DynamicDataViewConfiguration Default_32x32 = new DynamicDataViewConfiguration(0, 0, 32, 32);
        public static readonly DynamicDataViewConfiguration Default_16x16 = new DynamicDataViewConfiguration(0, 0, 16, 16);
        
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly int OffsetX;

        [DataMember(Order = 1)]
        [Key(1)]
        public readonly int OffsetY;

        [DataMember(Order = 2)]
        [Key(2)]
        public readonly int TileSizeX;

        [DataMember(Order = 3)]
        [Key(3)]
        public readonly int TileSizeY;

        public DynamicDataViewConfiguration(int offsetX, int offsetY, int tileSizeX, int tileSizeY)
        {
            if (tileSizeX <= 0) throw new ArgumentException(nameof(tileSizeX));
            if (tileSizeY <= 0) throw new ArgumentException(nameof(tileSizeY));

            OffsetX = offsetX;
            OffsetY = offsetY;
            TileSizeX = tileSizeX;
            TileSizeY = tileSizeY;
        }

        public bool Equals(DynamicDataViewConfiguration other)
        {
            return OffsetX == other.OffsetX && OffsetY == other.OffsetY && TileSizeX == other.TileSizeX && TileSizeY == other.TileSizeY;
        }

        public override bool Equals(object obj)
        {
            return obj is DynamicDataViewConfiguration other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = OffsetX;
                hashCode = (hashCode * 397) ^ OffsetY;
                hashCode = (hashCode * 397) ^ TileSizeX;
                hashCode = (hashCode * 397) ^ TileSizeY;
                return hashCode;
            }
        }

        public static bool operator ==(DynamicDataViewConfiguration left, DynamicDataViewConfiguration right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DynamicDataViewConfiguration left, DynamicDataViewConfiguration right)
        {
            return !left.Equals(right);
        }

        public Position2D TileIndex(int x, int y)
        {
            var dx = DataViewPartitions.TileSplit(x, OffsetX, TileSizeX);
            var dy = DataViewPartitions.TileSplit(y, OffsetY, TileSizeY);
            return new Position2D(dx, dy);
        }
        
        public (Position2D, Rectangle) Configure(int x, int y)
        {
            var dx = DataViewPartitions.TileSplit(x, OffsetX, TileSizeX);
            var dy = DataViewPartitions.TileSplit(y, OffsetY, TileSizeY);
            return (new Position2D(dx, dy), new Rectangle(dx * TileSizeX + OffsetX, dy * TileSizeY + OffsetY, TileSizeX, TileSizeY));
        }
    }

    public static class DynamicDataViewConfigurationExtensions
    {
        public static DynamicDataViewConfiguration ToConfiguration<T>(this IReadOnlyDynamicDataView3D<T> view)
        {
            return new DynamicDataViewConfiguration(view.OffsetX, view.OffsetY, view.TileSizeX, view.TileSizeY);
        }
        
        public static DynamicDataViewConfiguration ToConfiguration<T>(this IReadOnlyDynamicDataView2D<T> view)
        {
            return new DynamicDataViewConfiguration(view.OffsetX, view.OffsetY, view.TileSizeX, view.TileSizeY);
        }
    }
}