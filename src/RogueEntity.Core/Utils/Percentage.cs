using System;
using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Utils
{
    [Serializable]
    [DataContract]
    [MessagePackObject]
    public readonly struct Percentage : IEquatable<Percentage>, IComparable<Percentage>, IComparable
    {
        public static readonly Percentage Empty = new Percentage(0f);
        public static readonly Percentage Full = new Percentage(1f);

        [DataMember(Order=0)]
        [Key(0)]
        public readonly byte RawData;

        [SerializationConstructor]
        Percentage(byte rawData)
        {
            if (rawData > 200)
            {
                rawData = 200;
            }
            this.RawData = rawData;
        }

        public Percentage(float data)
        {
            if (data < 0) data = 0;
            if (data > 1) data = 1;
            this.RawData = (byte) (200 * data);
        }

        public static implicit operator float(Percentage p)
        {
            return p.RawData / 200f;
        }

        public float ToFloat()
        {
            return this;
        }

        public bool Equals(Percentage other)
        {
            return RawData == other.RawData;
        }

        public override bool Equals(object obj)
        {
            return obj is Percentage other && Equals(other);
        }

        public override int GetHashCode()
        {
            return RawData.GetHashCode();
        }

        public static Percentage operator +(Percentage left, Percentage right)
        {
            return new Percentage((byte)(left.RawData + right.RawData).Clamp(0, 200));
        }

        public static bool operator ==(Percentage left, Percentage right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Percentage left, Percentage right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{ToFloat():P}";
        }

        public int CompareTo(Percentage other)
        {
            return RawData.CompareTo(other.RawData);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            return obj is Percentage other
                ? CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(Percentage)}");
        }

        public static bool operator <(Percentage left, Percentage right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(Percentage left, Percentage right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(Percentage left, Percentage right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(Percentage left, Percentage right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static Percentage Of(float valueOto1)
        {
            return new Percentage(valueOto1);
        }

        public static Percentage Of(double valueOto1)
        {
            return new Percentage((float) valueOto1);
        }

        public static Percentage FromRaw(byte rawData)
        {
            return new Percentage(rawData);
        }
    }
}