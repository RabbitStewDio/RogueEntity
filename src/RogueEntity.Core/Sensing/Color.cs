using System;

namespace RogueEntity.Core.Sensing
{
    public readonly struct Color : IEquatable<Color>
    {
        public readonly byte Red;
        public readonly byte Green;
        public readonly byte Blue;

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