using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Sources.Light;

namespace RogueEntity.Core.Sensing.Sources.Heat
{
    [EntityComponent]
    [DataContract]
    [MessagePackObject]
    public readonly struct HeatSourceDefinition: ISenseDefinition, IEquatable<HeatSourceDefinition>
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
        public HeatSourceDefinition(SenseSourceDefinition senseDefinition, bool enabled)
        {
            SenseDefinition = senseDefinition;
            Enabled = enabled;
        }

        public HeatSourceDefinition WithSenseSource(SenseSourceDefinition sense)
        {
            return new HeatSourceDefinition(sense, Enabled);
        }

        public HeatSourceDefinition WithEnabled(bool enabled = true)
        {
            return new HeatSourceDefinition(SenseDefinition, Enabled);
        }

        public bool Equals(HeatSourceDefinition other)
        {
            return SenseDefinition.Equals(other.SenseDefinition) && Enabled == other.Enabled;
        }

        public override bool Equals(object obj)
        {
            return obj is HeatSourceDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (SenseDefinition.GetHashCode() * 397) ^ Enabled.GetHashCode();
            }
        }

        public static bool operator ==(HeatSourceDefinition left, HeatSourceDefinition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HeatSourceDefinition left, HeatSourceDefinition right)
        {
            return !left.Equals(right);
        }
    }
}