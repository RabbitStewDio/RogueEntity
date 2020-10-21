using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Resistance
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public readonly struct SensoryResistance : IEquatable<SensoryResistance>
    {
        [DataMember(Order = 0)]
        public readonly Percentage BlocksLight;
        [DataMember(Order = 1)]
        public readonly Percentage BlocksSound;
        [DataMember(Order = 2)]
        public readonly Percentage BlocksHeat;
        [DataMember(Order = 3)]
        public readonly Percentage BlocksSmell;

        [SerializationConstructor]
        public SensoryResistance(Percentage blocksLight, 
                                 Percentage blocksSound, 
                                 Percentage blocksHeat, 
                                 Percentage blocksSmell)
        {
            BlocksLight = blocksLight;
            BlocksSound = blocksSound;
            BlocksHeat = blocksHeat;
            BlocksSmell = blocksSmell;
        }

        public override string ToString()
        {
            return $"{nameof(BlocksLight)}: {BlocksLight}, {nameof(BlocksSound)}: {BlocksSound}, {nameof(BlocksHeat)}: {BlocksHeat}, {nameof(BlocksSmell)}: {BlocksSmell}";
        }

        public bool Equals(SensoryResistance other)
        {
            return BlocksLight.Equals(other.BlocksLight) && BlocksSound.Equals(other.BlocksSound) && BlocksHeat.Equals(other.BlocksHeat) && BlocksSmell.Equals(other.BlocksSmell);
        }

        public override bool Equals(object obj)
        {
            return obj is SensoryResistance other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = BlocksLight.GetHashCode();
                hashCode = (hashCode * 397) ^ BlocksSound.GetHashCode();
                hashCode = (hashCode * 397) ^ BlocksHeat.GetHashCode();
                hashCode = (hashCode * 397) ^ BlocksSmell.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SensoryResistance left, SensoryResistance right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SensoryResistance left, SensoryResistance right)
        {
            return !left.Equals(right);
        }
        
        public static SensoryResistance operator +(SensoryResistance left, SensoryResistance right)
        {
            return new SensoryResistance(left.BlocksLight + right.BlocksLight, 
                                         left.BlocksSound + right.BlocksSound,
                                         left.BlocksHeat + right.BlocksHeat,
                                         left.BlocksSmell + right.BlocksSmell
            );
        }


    }
}