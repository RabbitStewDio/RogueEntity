using MessagePack;
using System;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Utils
{
    [MessagePackObject()]
    [DataContract]
    public readonly struct Dimension : IEquatable<Dimension>
    {
        public static readonly Dimension Empty = default;
        
        [Key(0)]
        [DataMember(Order = 0)]
        public readonly int Width;
        [Key(1)]
        [DataMember(Order = 1)]
        public readonly int Height;

        public Dimension(int width, int height)
        {
            Width = width;
            Height = height;
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public int Area => Width * Height;
        
        public void Deconstruct(out int width, out int height)
        {
            width = Width;
            height = Height;
        }

        public bool Equals(Dimension other)
        {
            return Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            return obj is Dimension other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Width * 397) ^ Height;
            }
        }

        public static bool operator ==(Dimension left, Dimension right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Dimension left, Dimension right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"({nameof(Width)}: {Width}, {nameof(Height)}: {Height})";
        }
    }
}