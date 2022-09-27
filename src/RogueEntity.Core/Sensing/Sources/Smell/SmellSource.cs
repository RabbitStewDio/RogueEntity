using System;
using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Sensing.Sources.Smell
{
    [DataContract]
    [MessagePackObject]
    public readonly struct SmellSource : IEquatable<SmellSource>
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly float Intensity;
        [DataMember(Order = 1)]
        [Key(1)]
        public readonly string? Tag;

        [SerializationConstructor]
        public SmellSource(float intensity, string? tag = null)
        {
            Intensity = intensity;
            Tag = tag;
        }

        public bool Equals(SmellSource other)
        {
            return Intensity.Equals(other.Intensity) && Tag == other.Tag;
        }

        public override bool Equals(object obj)
        {
            return obj is SmellSource other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Intensity.GetHashCode() * 397) ^ (Tag != null ? Tag.GetHashCode() : 0);
            }
        }

        public static bool operator ==(SmellSource left, SmellSource right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SmellSource left, SmellSource right)
        {
            return !left.Equals(right);
        }
    }
}