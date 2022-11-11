using System;
using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Utils.DataViews
{
    [MessagePackObject]
    [DataContract]
    public readonly struct DynamicDataViewConfiguration : IEquatable<DynamicDataViewConfiguration>
    {
        public static readonly DynamicDataViewConfiguration Default64X64 = new DynamicDataViewConfiguration(0, 0, 64, 64);
        public static readonly DynamicDataViewConfiguration Default32X32 = new DynamicDataViewConfiguration(0, 0, 32, 32);
        public static readonly DynamicDataViewConfiguration Default16X16 = new DynamicDataViewConfiguration(0, 0, 16, 16);
        
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

        public Position2D Position(TileIndex t)
        {
            return new Position2D(t.X * TileSizeX + OffsetX, t.Y * TileSizeY + OffsetY);
        }

        public TileIndex TileIndex(int x, int y)
        {
            var dx = DataViewPartitions.TileSplit(x, OffsetX, TileSizeX);
            var dy = DataViewPartitions.TileSplit(y, OffsetY, TileSizeY);
            return new TileIndex(dx, dy);
        }
        
        public (TileIndex, Rectangle) Configure(int x, int y)
        {
            var dx = DataViewPartitions.TileSplit(x, OffsetX, TileSizeX);
            var dy = DataViewPartitions.TileSplit(y, OffsetY, TileSizeY);
            return (new TileIndex(dx, dy), new Rectangle(dx * TileSizeX + OffsetX, dy * TileSizeY + OffsetY, TileSizeX, TileSizeY));
        }

        public Rectangle GetDefaultBounds()
        {
            return new Rectangle(OffsetX, OffsetY, TileSizeX, TileSizeY);
        }
        
        public override string ToString()
        {
            return $"{nameof(DynamicDataViewConfiguration)}({nameof(OffsetX)}: {OffsetX}, {nameof(OffsetY)}: {OffsetY}, {nameof(TileSizeX)}: {TileSizeX}, {nameof(TileSizeY)}: {TileSizeY})";
        }
    }

    public readonly struct TileIndex : IEquatable<TileIndex>
    {
        public readonly int X;
        public readonly int Y;

        public TileIndex(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(TileIndex other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is TileIndex other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public static bool operator ==(TileIndex left, TileIndex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TileIndex left, TileIndex right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{nameof(TileIndex)}({nameof(X)}: {X}, {nameof(Y)}: {Y})";
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