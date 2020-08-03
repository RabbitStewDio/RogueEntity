using System;
using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Utils
{
    [Serializable]
    [DataContract]
    [MessagePackObject]
    public readonly struct RgbColor : IEquatable<RgbColor>
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly int Red;
        [DataMember(Order = 1)]
        [Key(1)]
        public readonly int Green;
        [DataMember(Order = 2)]
        [Key(2)]
        public readonly int Blue;

        [SerializationConstructor]
        public RgbColor(int red, int green, int blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public bool Equals(RgbColor other)
        {
            return Red == other.Red && Green == other.Green && Blue == other.Blue;
        }

        public override bool Equals(object obj)
        {
            return obj is RgbColor other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Red;
                hashCode = (hashCode * 397) ^ Green;
                hashCode = (hashCode * 397) ^ Blue;
                return hashCode;
            }
        }

        public static bool operator ==(RgbColor left, RgbColor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RgbColor left, RgbColor right)
        {
            return !left.Equals(right);
        }
    }
}