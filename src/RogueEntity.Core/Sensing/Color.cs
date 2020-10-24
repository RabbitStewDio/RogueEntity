using System;
using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Sensing
{
    [DataContract]
    [MessagePackObject]
    public readonly struct Color : IEquatable<Color>
    {
        [Key(0)]
        [DataMember(Order = 0)]
        public readonly byte Red;
        [Key(1)]
        [DataMember(Order = 1)]
        public readonly byte Green;
        [Key(2)]
        [DataMember(Order = 2)]
        public readonly byte Blue;

        [SerializationConstructor]
        public Color(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public bool Equals(Color other)
        {
            return Red == other.Red && Green == other.Green && Blue == other.Blue;
        }

        public override bool Equals(object obj)
        {
            return obj is Color other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Red.GetHashCode();
                hashCode = (hashCode * 397) ^ Green.GetHashCode();
                hashCode = (hashCode * 397) ^ Blue.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Color left, Color right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Color left, Color right)
        {
            return !left.Equals(right);
        }
    }
}