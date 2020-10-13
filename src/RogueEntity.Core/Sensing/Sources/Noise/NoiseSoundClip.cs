using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [DataContract]
    [MessagePackObject]
    public readonly struct NoiseSoundClip : IEquatable<NoiseSoundClip>
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
        public NoiseSoundClip(float intensity, string tag = default)
        {
            Intensity = intensity;
            Tag = tag;
        }

        public bool Equals(NoiseSoundClip other)
        {
            return Intensity.Equals(other.Intensity) && Tag == other.Tag;
        }

        public override bool Equals(object obj)
        {
            return obj is NoiseSoundClip other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Intensity.GetHashCode() * 397) ^ (Tag != null ? Tag.GetHashCode() : 0);
            }
        }

        public static bool operator ==(NoiseSoundClip left, NoiseSoundClip right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NoiseSoundClip left, NoiseSoundClip right)
        {
            return !left.Equals(right);
        }
    }
}