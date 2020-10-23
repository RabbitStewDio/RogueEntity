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
        readonly byte data;

        [SerializationConstructor]
        Percentage(byte data)
        {
            if (data > 200)
            {
                data = 200;
            }
            this.data = data;
        }

        public Percentage(float data)
        {
            if (data < 0) data = 0;
            if (data > 1) data = 1;
            this.data = (byte) (200 * data);
        }

        public static implicit operator float(Percentage p)
        {
            return p.data / 200f;
        }

        public float ToFloat()
        {
            return this;
        }

        public bool Equals(Percentage other)
        {
            return data == other.data;
        }

        public override bool Equals(object obj)
        {
            return obj is Percentage other && Equals(other);
        }

        public override int GetHashCode()
        {
            return data.GetHashCode();
        }

        public static Percentage operator +(Percentage left, Percentage right)
        {
            return new Percentage((byte)(left.data + right.data).Clamp(0, 200));
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
            return data.CompareTo(other.data);
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

        public byte ToRawData() => data;
    }
}