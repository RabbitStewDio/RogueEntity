using System;

namespace RogueEntity.Core.Utils
{
    public struct RgbColor : IEquatable<RgbColor>
    {
        public readonly int Red;
        public readonly int Green;
        public readonly int Blue;

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