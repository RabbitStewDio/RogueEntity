using System;
using System.Runtime.Serialization;
using EnTTSharp.Annotations;
using MessagePack;

namespace RogueEntity.Core.Infrastructure.Meta
{
    /// <summary>
    ///  Temperature is given in 10ths of degrees kelvin. Nothing can be colder than 0K,
    ///  and temperature is limited to 6500K (still hotter than the surface of the sun at 5800k).
    /// </summary>
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public readonly struct Temperature : IComparable<Temperature>, IComparable, IEquatable<Temperature>
    {
        [DataMember(Order = 0)]
        [Key(0)]
        readonly ushort data;

        [SerializationConstructor]
        Temperature(ushort data)
        {
            this.data = data;
        }

        public float ToCelsius() => (data * 10f - 273.1f);
        public static Temperature FromCelsius(float c) => new Temperature((ushort) ((c + 273.1f) / 10f));

        public bool Equals(Temperature other)
        {
            return data == other.data;
        }

        public override bool Equals(object obj)
        {
            return obj is Temperature other && Equals(other);
        }

        public override int GetHashCode()
        {
            return data.GetHashCode();
        }

        public static bool operator ==(Temperature left, Temperature right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Temperature left, Temperature right)
        {
            return !left.Equals(right);
        }

        public int CompareTo(Temperature other)
        {
            return data.CompareTo(other.data);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            return obj is Temperature other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Temperature)}");
        }

        public static bool operator <(Temperature left, Temperature right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(Temperature left, Temperature right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(Temperature left, Temperature right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(Temperature left, Temperature right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static Temperature operator +(Temperature left, Temperature right)
        {
            return new Temperature((ushort) (left.data + right.data));
        }

        public static Temperature operator -(Temperature left, Temperature right)
        {
            return new Temperature((ushort) (left.data - right.data));
        }

        public static Temperature operator *(Temperature left, float right)
        {
            return new Temperature((ushort) (left.data * right));
        }

        public static Temperature operator /(Temperature left, float right)
        {
            return new Temperature((ushort) (left.data / right));
        }

        public override string ToString()
        {
            return $"{ToCelsius()}C";
        }
    }
}