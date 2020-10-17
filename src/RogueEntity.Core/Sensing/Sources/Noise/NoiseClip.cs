using System;
using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    [DataContract]
    [MessagePackObject]
    public readonly struct NoiseClip : IEquatable<NoiseClip>
    {
        [Key(0)]
        [DataMember(Order = 0)]
        public readonly float Intensity;
        
        /// <summary>
        ///   A tag used to select sounds or other effects in the client.
        /// </summary>
        [Key(1)]
        [DataMember(Order = 1)]
        public readonly string Tag;

        [SerializationConstructor]
        public NoiseClip(float intensity, string tag = default)
        {
            Intensity = intensity;
            Tag = tag;
        }

        public bool Equals(NoiseClip other)
        {
            return Intensity.Equals(other.Intensity) && Tag == other.Tag;
        }

        public override bool Equals(object obj)
        {
            return obj is NoiseClip other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Intensity.GetHashCode() * 397) ^ (Tag != null ? Tag.GetHashCode() : 0);
            }
        }

        public static bool operator ==(NoiseClip left, NoiseClip right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NoiseClip left, NoiseClip right)
        {
            return !left.Equals(right);
        }
    }
}