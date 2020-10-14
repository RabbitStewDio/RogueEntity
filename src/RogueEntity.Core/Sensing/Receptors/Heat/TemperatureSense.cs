using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Sensing.Receptors.Light;

namespace RogueEntity.Core.Sensing.Receptors.Temperature
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [DataContract]
    [MessagePackObject]
    public readonly struct TemperatureSense: ISense
    {
        [Key(0)]
        [DataMember(Order = 0)]
        readonly float senseRadius;
        [Key(1)]
        [DataMember(Order = 1)]
        readonly float senseStrength;

        [SerializationConstructor]
        public TemperatureSense(float senseRadius, float senseStrength)
        {
            this.senseRadius = senseRadius;
            this.senseStrength = senseStrength;
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public float SenseRadius => senseRadius;
        
        [IgnoreMember]
        [IgnoreDataMember]
        public float SenseStrength => senseStrength;

        public bool Equals(TemperatureSense other)
        {
            return senseRadius.Equals(other.senseRadius) && senseStrength.Equals(other.senseStrength);
        }

        public override bool Equals(object obj)
        {
            return obj is TemperatureSense other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (senseRadius.GetHashCode() * 397) ^ senseStrength.GetHashCode();
            }
        }

        public static bool operator ==(TemperatureSense left, TemperatureSense right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TemperatureSense left, TemperatureSense right)
        {
            return !left.Equals(right);
        }
        
    }
}