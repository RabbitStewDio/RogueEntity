using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Positioning.Grid
{
    /// <summary>
    ///   A densely packed 4D tile coordinate. The coordinate encodes a 3D coordinate with
    ///   unsigned members and a maximum extent of x=2^16, y=2^16, z=2^16 (aka x=64k, y=64k, z=64k) meters
    ///   and 7 layers for each coordinate point.
    ///
    ///   Fully instantiated such a map would consume gigabytes, so its safe to assume that these limits are sufficient.
    /// </summary>
    [EntityComponent]
    [DataContract]
    [Serializable]
    [MessagePackObject]
    public readonly struct EntityGridPosition : IEquatable<EntityGridPosition>, IPosition
    {
        public const int MinXYValue = short.MinValue;
        public const int MaxXYValue = short.MaxValue;
        public const int MinZValue = short.MinValue;
        public const int MaxZValue = short.MaxValue;
        
        public const int MinLValue = byte.MinValue;
        public const int MaxLValue = byte.MaxValue;

        [DataMember]
        [Key(0)]
        readonly byte layerId;
        [DataMember]
        [Key(1)]
        readonly short x;
        [DataMember]
        [Key(2)]
        readonly short y;
        [DataMember]
        [Key(3)]
        readonly short z;
        [DataMember]
        [Key(4)]
        readonly byte valid;

        [SerializationConstructor]
        public EntityGridPosition(byte layerId, short x, short y, short z, byte valid = 1)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.layerId = layerId;
            this.valid = valid == 0 ? (byte)0 : (byte)1;
        }

        public static EntityGridPosition From<TPosition>(in TPosition p)
            where TPosition : IPosition
        {
            if (p.IsInvalid) return Invalid;
            return OfRaw(p.LayerId, p.GridX, p.GridY, p.GridZ);
        }

        public static EntityGridPosition Of(MapLayer layer, int x, int y, int z = 0)
        {
            if (x < MinXYValue) throw new ArgumentOutOfRangeException(nameof(x), x, $"should be between {MinXYValue} and {MaxXYValue}");
            if (y < MinXYValue) throw new ArgumentOutOfRangeException(nameof(y), y, $"should be between {MinXYValue} and {MaxXYValue}");
            if (z < MinZValue) throw new ArgumentOutOfRangeException(nameof(z), z, $"should be between {MinXYValue} and {MaxXYValue}");
            if (x > MaxXYValue) throw new ArgumentOutOfRangeException(nameof(x), x, $"should be between {MinXYValue} and {MaxXYValue}");
            if (y > MaxXYValue) throw new ArgumentOutOfRangeException(nameof(y), y, $"should be between {MinXYValue} and {MaxXYValue}");
            if (z > MaxZValue) throw new ArgumentOutOfRangeException(nameof(z), z, $"should be between {MinZValue} and {MaxZValue}");

            return new EntityGridPosition(layer.LayerId, (short)x, (short)y, (short)z);
        }

        public static EntityGridPosition OfRaw(byte layer, int x, int y, int z = 0)
        {
            if (x < MinXYValue) throw new ArgumentOutOfRangeException(nameof(x), x, $"should be between {MinXYValue} and {MaxXYValue}");
            if (y < MinXYValue) throw new ArgumentOutOfRangeException(nameof(y), y, $"should be between {MinXYValue} and {MaxXYValue}");
            if (z < MinZValue) throw new ArgumentOutOfRangeException(nameof(z), z, $"should be between {MinXYValue} and {MaxXYValue}");
            if (x > MaxXYValue) throw new ArgumentOutOfRangeException(nameof(x), x, $"should be between {MinXYValue} and {MaxXYValue}");
            if (y > MaxXYValue) throw new ArgumentOutOfRangeException(nameof(y), y, $"should be between {MinXYValue} and {MaxXYValue}");
            if (z > MaxZValue) throw new ArgumentOutOfRangeException(nameof(z), z, $"should be between {MinZValue} and {MaxZValue}");

            return new EntityGridPosition(layer, (short)x, (short)y, (short)z, 1);
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public bool IsInvalid => this.valid != 0;

        public static EntityGridPosition Invalid => default;

        public bool Equals(EntityGridPosition other)
        {
            return layerId == other.layerId && x == other.x && y == other.y && z == other.z && valid == other.valid;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityGridPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = layerId.GetHashCode();
                hashCode = (hashCode * 397) ^ x.GetHashCode();
                hashCode = (hashCode * 397) ^ y.GetHashCode();
                hashCode = (hashCode * 397) ^ z.GetHashCode();
                hashCode = (hashCode * 397) ^ valid.GetHashCode();
                return hashCode;
            }
        }

        public static bool IsSameCell(EntityGridPosition left, EntityGridPosition right)
        {
            if (left == Invalid || right == Invalid)
            {
                return false;
            }

            return (left.x == right.x && left.y == right.y && left.z == right.z);
        }

        public static bool operator ==(EntityGridPosition left, EntityGridPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityGridPosition left, EntityGridPosition right)
        {
            return !left.Equals(right);
        }

        public static EntityGridPosition operator +(EntityGridPosition left, Position2D right)
        {
            return OfRaw(left.LayerId,
                         left.GridX + right.X,
                         left.GridY + right.Y,
                         left.GridZ);
        }

        public static EntityGridPosition operator +(EntityGridPosition left, ShortPosition2D right)
        {
            return OfRaw(left.LayerId,
                         left.GridX + right.X,
                         left.GridY + right.Y,
                         left.GridZ);
        }

        public override string ToString()
        {
            if (IsInvalid)
            {
                return "(Invalid)";
            }

            return $"({LayerId}: {GridX}, {GridY}, {GridZ})";
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public byte LayerId
        {
            get
            {
                if (IsInvalid)
                {
                    throw new ArgumentException("Invalid");
                }

                return layerId;
            }
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public double X => GridX;

        [IgnoreMember]
        [IgnoreDataMember]
        public double Y => GridY;

        [IgnoreMember]
        [IgnoreDataMember]
        public double Z => GridZ;

        [IgnoreMember]
        [IgnoreDataMember]
        public int GridX
        {
            get
            {
                if (IsInvalid)
                {
                    throw new ArgumentException("Invalid");
                }

                return x;
            }
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public int GridY
        {
            get
            {
                if (IsInvalid)
                {
                    throw new ArgumentException("Invalid");
                }

                return y;
            }
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public int GridZ
        {
            get
            {
                if (IsInvalid)
                {
                    throw new ArgumentException("Invalid");
                }

                return z;
            }
        }

        public EntityGridPosition WithLayer(in MapLayer layer)
        {
            return OfRaw(layer.LayerId, GridX, GridY, GridZ);
        }

        public EntityGridPosition WithLayer(in byte layer)
        {
            return OfRaw(layer, GridX, GridY, GridZ);
        }
    }
}