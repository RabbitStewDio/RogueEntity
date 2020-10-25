using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Sources.Smell
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public readonly struct SmellSourceDefinition: ISenseDefinition
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly SenseSourceDefinition SenseDefinition;

        [DataMember(Order = 1)]
        [Key(1)]
        public readonly SmellSource Smell;

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
        public SmellSourceDefinition(SenseSourceDefinition senseDefinition, SmellSource smell, bool enabled)
        {
            SenseDefinition = senseDefinition;
            Smell = smell;
            Enabled = enabled;
        }

        public SmellSourceDefinition WithSenseSource(SenseSourceDefinition sense)
        {
            return new SmellSourceDefinition(sense, Smell, Enabled);
        }

        public SmellSourceDefinition WithSmell(SmellSource clip)
        {
            return new SmellSourceDefinition(SenseDefinition, clip, Enabled);
        }

        public SmellSourceDefinition WithEnabled(bool enabled = true)
        {
            return new SmellSourceDefinition(SenseDefinition, Smell, enabled);
        }

        public bool Equals(SmellSourceDefinition other)
        {
            return SenseDefinition.Equals(other.SenseDefinition) && Enabled == other.Enabled;
        }

        public override bool Equals(object obj)
        {
            return obj is SmellSourceDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (SenseDefinition.GetHashCode() * 397) ^ Enabled.GetHashCode();
            }
        }

        public static bool operator ==(SmellSourceDefinition left, SmellSourceDefinition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SmellSourceDefinition left, SmellSourceDefinition right)
        {
            return !left.Equals(right);
        }
    }
}