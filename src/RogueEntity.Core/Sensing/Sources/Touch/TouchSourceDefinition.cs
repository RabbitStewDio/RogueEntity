using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Sources.Touch
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public readonly struct TouchSourceDefinition: ISenseDefinition
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly SenseSourceDefinition SenseDefinition;

        [DataMember(Order = 1)]
        [Key(1)]
        public readonly bool Enabled;

        [IgnoreMember]
        [IgnoreDataMember]
        SenseSourceDefinition ISenseDefinition.SenseDefinition => SenseDefinition;

        [IgnoreMember]
        [IgnoreDataMember]
        bool ISenseDefinition.Enabled => Enabled;

        [SerializationConstructor]
        public TouchSourceDefinition(SenseSourceDefinition senseDefinition, bool enabled)
        {
            SenseDefinition = senseDefinition;
            Enabled = enabled;
        }

        public TouchSourceDefinition WithSenseSource(SenseSourceDefinition sense)
        {
            return new TouchSourceDefinition(sense, Enabled);
        }

        public TouchSourceDefinition WithEnabled(bool enabled = true)
        {
            return new TouchSourceDefinition(SenseDefinition, enabled);
        }

        public bool Equals(TouchSourceDefinition other)
        {
            return SenseDefinition.Equals(other.SenseDefinition) && Enabled == other.Enabled;
        }

        public override bool Equals(object obj)
        {
            return obj is TouchSourceDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (SenseDefinition.GetHashCode() * 397) ^ Enabled.GetHashCode();
            }
        }

        public static bool operator ==(TouchSourceDefinition left, TouchSourceDefinition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TouchSourceDefinition left, TouchSourceDefinition right)
        {
            return !left.Equals(right);
        }
        
    }
}