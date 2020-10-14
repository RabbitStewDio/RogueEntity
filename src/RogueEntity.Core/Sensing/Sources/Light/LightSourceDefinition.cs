using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    public interface ISenseDefinition
    {
        bool Enabled { get; }
        SenseSourceDefinition SenseDefinition { get; }
    }

    [EntityComponent]
    [DataContract]
    [MessagePackObject]
    public readonly struct LightSourceDefinition : ISenseDefinition, IEquatable<LightSourceDefinition>
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly SenseSourceDefinition SenseDefinition;

        [DataMember(Order = 1)]
        [Key(1)]
        public readonly float Hue;

        [DataMember(Order = 2)]
        [Key(2)]
        public readonly float Saturation;

        [DataMember(Order = 3)]
        [Key(3)]
        public readonly bool Enabled;

        [SerializationConstructor]
        public LightSourceDefinition(SenseSourceDefinition senseDefinition,
                                     float hue,
                                     float saturation,
                                     bool enabled = true)
        {
            Hue = hue;
            Saturation = saturation;
            SenseDefinition = senseDefinition;
            Enabled = enabled;
        }

        [IgnoreMember]
        [IgnoreDataMember]
        SenseSourceDefinition ISenseDefinition.SenseDefinition => SenseDefinition;

        [IgnoreMember]
        [IgnoreDataMember]
        bool ISenseDefinition.Enabled => Enabled;

        public LightSourceDefinition WithColour(float hue, float saturation = 1f)
        {
            return new LightSourceDefinition(SenseDefinition, hue, saturation, Enabled);
        }

        public LightSourceDefinition WithSenseSource(SenseSourceDefinition sense)
        {
            return new LightSourceDefinition(sense, Hue, Saturation, Enabled);
        }

        public LightSourceDefinition WithEnabled(bool enabled = true)
        {
            return new LightSourceDefinition(SenseDefinition, Hue, Saturation, Enabled);
        }

        public bool Equals(LightSourceDefinition other)
        {
            return SenseDefinition.Equals(other.SenseDefinition) && Hue.Equals(other.Hue) && Saturation.Equals(other.Saturation) && Enabled == other.Enabled;
        }

        public override bool Equals(object obj)
        {
            return obj is LightSourceDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SenseDefinition.GetHashCode();
                hashCode = (hashCode * 397) ^ Hue.GetHashCode();
                hashCode = (hashCode * 397) ^ Saturation.GetHashCode();
                hashCode = (hashCode * 397) ^ Enabled.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(LightSourceDefinition left, LightSourceDefinition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LightSourceDefinition left, LightSourceDefinition right)
        {
            return !left.Equals(right);
        }
    }
}