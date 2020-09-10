using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning.Grid
{
    /// <summary>
    ///   A densly packed 4D tile coordinate. The coordinate encodes a 3D coordinate with
    ///   unsigned members and a maximum extent of x=2^24, y=2^24, z=2^12 (aka x=16M, y=16M, z=4k) meters
    ///   and 7 layers for each coordinate point.
    /// </summary>
    [EntityComponent]
    [DataContract]
    [Serializable]
    [MessagePackObject]
    public readonly struct EntityGridPosition : IEquatable<EntityGridPosition>, IPosition
    {
        const int MaxXYValue = (1 << 24) - 1;
        const int MaxZValue = (1 << 12) - 1;
        const int MaxLValue = 7;

        const ulong XMask = 0x0000_0000_00FF_FFFFL;
        const ulong YMask = 0x0000_FFFF_FF00_0000L;
        const ulong ZMask = 0x0FFF_0000_0000_0000L;
        const ulong LMask = 0x7000_0000_0000_0000L;
        const ulong VMask = 0x8000_0000_0000_0000L;
        const ulong PMask = 0x0FFF_FFFF_FFFF_FFFFL;

        [DataMember]
        [Key(0)]
        readonly ulong position;

        [SerializationConstructor]
        EntityGridPosition(ulong position)
        {
            this.position = position;
        }

        EntityGridPosition(byte layer, uint x, uint y, uint z)
        {
            if (x > MaxXYValue)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }
            if (y > MaxXYValue)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }

            if (z > MaxZValue)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }
            if (layer > MaxLValue)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }

            ulong data = VMask;
            data |= x & XMask;
            data |= ((ulong)y << 24) & YMask;
            data |= ((ulong)z << 48) & ZMask;
            data |= ((ulong)layer << 60) & LMask;
            position = data;
        }

        public static EntityGridPosition From<TPosition>(in TPosition p) where TPosition: IPosition
        {
            if (p.IsInvalid) return Invalid;
            return OfRaw(p.LayerId, p.GridX, p.GridY, p.GridZ);
        }

        public static EntityGridPosition Of(MapLayer layer, int x, int y, int z = 0)
        {
            if (x < 0) throw new ArgumentException();
            if (y < 0) throw new ArgumentException();
            if (z < 0) throw new ArgumentException();

            return new EntityGridPosition(layer.LayerId, (uint) x, (uint)y, (uint)z);
        }

        public static EntityGridPosition OfRaw(byte layer, int x, int y, int z = 0)
        {
            if (x < 0) throw new ArgumentException();
            if (y < 0) throw new ArgumentException();
            if (z < 0) throw new ArgumentException();

            return new EntityGridPosition(layer, (uint) x, (uint)y, (uint)z);
        }

        public bool IsInvalid => LayerId == 0;

        public static EntityGridPosition Invalid => default;

        public bool Equals(EntityGridPosition other)
        {
            return position == other.position;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityGridPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return position.GetHashCode();
        }

        public static bool IsSameCell(EntityGridPosition left, EntityGridPosition right)
        {
            if (left == Invalid || right == Invalid)
            {
                return false;
            }

            return (left.position & PMask) == (right.position & PMask);
        }

        public static bool operator ==(EntityGridPosition left, EntityGridPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityGridPosition left, EntityGridPosition right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            if (position == 0)
            {
                return "(Invalid)";
            }
            return $"({LayerId}: {GridX}, {GridY}, {GridZ})";
        }

        public byte LayerId
        {
            get
            {
                if (position == 0)
                {
                    throw new ArgumentException("Invalid");
                }

                return (byte)((position & LMask) >> 60);
            }
        }

        public double X => GridX;
        public double Y => GridY;
        public double Z => GridZ;

        public int GridX
        {
            get
            {
                if (position == 0)
                {
                    throw new ArgumentException("Invalid");
                }

                return (int)(position & XMask);
            }
        }

        public int GridY
        {
            get
            {
                if (position == 0)
                {
                    throw new ArgumentException("Invalid");
                }

                return (int)((position & YMask) >> 24);
            }
        }

        public int GridZ
        {
            get
            {
                if (position == 0)
                {
                    throw new ArgumentException("Invalid");
                }

                return (int)((position & ZMask) >> 48);
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