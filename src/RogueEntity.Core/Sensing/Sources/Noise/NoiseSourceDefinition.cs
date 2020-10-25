using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public readonly struct NoiseSourceDefinition: ISenseDefinition
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly SenseSourceDefinition SenseDefinition;

        [DataMember(Order = 1)]
        [Key(1)]
        public readonly NoiseClip Clip;
        
        [DataMember(Order = 2)]
        [Key(2)]
        public readonly bool Enabled;

        [IgnoreMember]
        [IgnoreDataMember]
        SenseSourceDefinition ISenseDefinition.SenseDefinition => SenseDefinition;

        [IgnoreMember]
        [IgnoreDataMember]
        bool ISenseDefinition.Enabled => Enabled;

        [SerializationConstructor]
        public NoiseSourceDefinition(SenseSourceDefinition senseDefinition, NoiseClip clip, bool enabled)
        {
            SenseDefinition = senseDefinition;
            Clip = clip;
            Enabled = enabled;
        }

        public NoiseSourceDefinition WithSenseSource(SenseSourceDefinition sense)
        {
            return new NoiseSourceDefinition(sense, Clip, Enabled);
        }

        public NoiseSourceDefinition WithClip(NoiseClip clip)
        {
            return new NoiseSourceDefinition(SenseDefinition, clip, Enabled);
        }

        public NoiseSourceDefinition WithEnabled(bool enabled = true)
        {
            return new NoiseSourceDefinition(SenseDefinition, Clip, enabled);
        }

        public bool Equals(NoiseSourceDefinition other)
        {
            return SenseDefinition.Equals(other.SenseDefinition) && Enabled == other.Enabled;
        }

        public override bool Equals(object obj)
        {
            return obj is NoiseSourceDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (SenseDefinition.GetHashCode() * 397) ^ Enabled.GetHashCode();
            }
        }

        public static bool operator ==(NoiseSourceDefinition left, NoiseSourceDefinition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NoiseSourceDefinition left, NoiseSourceDefinition right)
        {
            return !left.Equals(right);
        }
        
    }
}