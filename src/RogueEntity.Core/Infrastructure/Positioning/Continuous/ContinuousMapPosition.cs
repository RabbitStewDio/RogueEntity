using System;
using System.Runtime.Serialization;
using EnTTSharp.Annotations;
using MessagePack;

namespace RogueEntity.Core.Infrastructure.Positioning.Continuous
{
    [EntityComponent]
    [DataContract]
    [Serializable]
    [MessagePackObject]
    public readonly struct ContinuousMapPosition: IPosition, IEquatable<ContinuousMapPosition>
    {
        const ulong MaxCoordinate = 0xFFFF_FFFF_FFFF;
        const float MaxCoordinateMeter = MaxCoordinate / 1000f;
        [DataMember(Order = 0)]
        [Key(0)]
        readonly ulong dataA;
        [DataMember(Order = 1)]
        [Key(1)]
        readonly ulong dataB;

        public double X => XUnit / 1000f;
        public double Y => YUnit / 1000f;
        public double Z => ZUnit / 1000f;

        public ulong XUnit => dataA & 0x0000_FFFF_FFFF_FFFF;
        public ulong YUnit => dataB & 0x0000_FFFF_FFFF_FFFF;

        public ulong ZUnit => ((dataA & 0xFFFF_0000_0000_0000) >> 48) |
                          ((dataB & 0xFF00_0000_0000_0000) >> 36);

        public byte LayerId => (byte)((dataB & 0x00FF_0000_0000_0000) >> 48);

        [SerializationConstructor]
        ContinuousMapPosition(ulong dataA, ulong dataB)
        {
            this.dataA = dataA;
            this.dataB = dataB;
        }

        public ContinuousMapPosition(ulong x, ulong y, ulong z, byte l)
        {
            dataA = ((z & 0xFFFFul) << 48) | (x & 0x0000_FFFF_FFFF_FFFF);
            dataB = ((z & 0xFF_0000ul) << 36) |
                    (x & 0x0000_FFFF_FFFF_FFFF) |
                    ((ulong)(l) << 48);
        }

        public ContinuousMapPosition(double x, double y, double z, byte l) :
            this(FloatToMillimeter(x), FloatToMillimeter(y), FloatToMillimeter(z), l)

        {
        }

        public ContinuousMapPosition(in Position p) :
            this(FloatToMillimeter(p.X), FloatToMillimeter(p.Y), FloatToMillimeter(p.Z), p.LayerId)
        {
        }

        public int GridX => (int)(X + 0.5f);
        public int GridY => (int)(Y + 0.5f);
        public int GridZ => (int)(Z + 0.5f);

        static ulong FloatToMillimeter(double value)
        {
            if (value < 0) value = 0;
            if (value > MaxCoordinateMeter) value = MaxCoordinateMeter;

            return (ulong)(value / 1000f);
        }

        public static ContinuousMapPosition Invalid => default;

        public bool Equals(ContinuousMapPosition other)
        {
            return dataA == other.dataA && dataB == other.dataB;
        }

        public override bool Equals(object obj)
        {
            return obj is ContinuousMapPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (dataA.GetHashCode() * 397) ^ dataB.GetHashCode();
            }
        }

        public static bool operator ==(ContinuousMapPosition left, ContinuousMapPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ContinuousMapPosition left, ContinuousMapPosition right)
        {
            return !left.Equals(right);
        }
    }
}