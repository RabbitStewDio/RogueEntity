using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Positioning.Continuous
{
    /// <summary>
    ///   A fixed point representation of a continuous position.
    ///
    ///   Allows for a maximum range of 2^21 [2M] meters on X and Y, and 2^14 [8K] meters on the Z axis. All coordinates have
    ///   a resolution of 1024 Units per meter.
    /// </summary>
    [EntityComponent]
    [DataContract]
    [Serializable]
    [MessagePackObject]
    public readonly struct ContinuousMapPosition : IPosition<ContinuousMapPosition>, IEquatable<ContinuousMapPosition>
    {
        const double UnitScale = 1024;
        const int MaxZUnit = 0x7F_FFFF;
        const int ZMask = 0xFF_FFFF;

        const double MaxXY = int.MaxValue / UnitScale;
        const double MaxZ = MaxZUnit / UnitScale;
        
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly int XUnit;

        [DataMember(Order = 1)]
        [Key(1)]
        public readonly int YUnit;

        readonly uint zUnitAndLayerId;

        public double X => XUnit / UnitScale;
        public double Y => YUnit / UnitScale;
        public double Z => ZUnit / UnitScale;

        public int ZUnit => (int) (zUnitAndLayerId & ZMask) - MaxZUnit;

        public byte LayerId => (byte)((zUnitAndLayerId & 0xFF00_0000) >> 24);

        [SerializationConstructor]
        public ContinuousMapPosition(int xUnit, int yUnit, uint zUnitAndLayerId)
        {
            XUnit = xUnit;
            YUnit = yUnit;
            this.zUnitAndLayerId = zUnitAndLayerId;
        }

        ContinuousMapPosition(int x, int y, int z, byte l)
        {
            XUnit = x;
            YUnit = y;
            var zz = (z + MaxZUnit).Clamp(0, ZMask); 
            zUnitAndLayerId = (uint) ((l << 24) & zz);
        }

        public ContinuousMapPosition(double x, double y, double z, byte l) :
            this(FloatToMillimeter(x, MaxXY), FloatToMillimeter(y, MaxXY), FloatToMillimeter(z, MaxZ), l)
        {
        }

        public static ContinuousMapPosition From(in Position p)
        {
            if (p.IsInvalid) return Invalid;
            return new ContinuousMapPosition(FloatToMillimeter(p.X, MaxXY), FloatToMillimeter(p.Y, MaxXY), FloatToMillimeter(p.Z, MaxZ), p.LayerId);
        }

        public int GridX => (int)(X + 0.5f);
        public int GridY => (int)(Y + 0.5f);
        public int GridZ => (int)(Z + 0.5f);

        static int FloatToMillimeter(double value, double maxValue)
        {
            if (value < 0) value = 0;
            if (value > maxValue) value = maxValue;

            return (int) Math.Round(value / UnitScale, MidpointRounding.AwayFromZero);
        }

        public bool IsInvalid => LayerId == 0;

        public static ContinuousMapPosition Invalid => default;


        public override bool Equals(object obj)
        {
            return obj is ContinuousMapPosition other && Equals(other);
        }

        public bool Equals(ContinuousMapPosition other)
        {
            return XUnit == other.XUnit && YUnit == other.YUnit && zUnitAndLayerId == other.zUnitAndLayerId;
        }

        public ContinuousMapPosition WithPosition(int x, int y)
        {
            return new ContinuousMapPosition(x, y, this.Z, this.LayerId);
        }

        public ContinuousMapPosition WithPosition(double tx, double ty)
        {
            return new ContinuousMapPosition(tx, ty, this.Z, this.LayerId);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = XUnit;
                hashCode = (hashCode * 397) ^ YUnit;
                hashCode = (hashCode * 397) ^ (int) zUnitAndLayerId;
                return hashCode;
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