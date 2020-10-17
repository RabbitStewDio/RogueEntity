using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Sources;

namespace RogueEntity.Core.Sensing.Receptors
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [DataContract]
    [MessagePackObject]
    public readonly struct SensoryReceptorData<TSense>: ISenseDefinition, IEquatable<SensoryReceptorData<TSense>>
        where TSense : ISense
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
        public SensoryReceptorData(SenseSourceDefinition senseDefinition, bool enabled)
        {
            SenseDefinition = senseDefinition;
            Enabled = enabled;
        }

        public SensoryReceptorData<TSense> WithSenseSource(SenseSourceDefinition sense)
        {
            return new SensoryReceptorData<TSense>(sense, Enabled);
        }

        public SensoryReceptorData<TSense> WithEnabled(bool enabled = true)
        {
            return new SensoryReceptorData<TSense>(SenseDefinition, enabled);
        }

        public bool Equals(SensoryReceptorData<TSense> other)
        {
            return SenseDefinition.Equals(other.SenseDefinition) && Enabled == other.Enabled;
        }

        public override bool Equals(object obj)
        {
            return obj is SensoryReceptorData<TSense> other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (SenseDefinition.GetHashCode() * 397) ^ Enabled.GetHashCode();
            }
        }

        public static bool operator ==(SensoryReceptorData<TSense> left, SensoryReceptorData<TSense> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SensoryReceptorData<TSense> left, SensoryReceptorData<TSense> right)
        {
            return !left.Equals(right);
        }
    }
}