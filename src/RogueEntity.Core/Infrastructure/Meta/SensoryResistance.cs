using System.Runtime.Serialization;
using EnTTSharp.Annotations;
using MessagePack;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Infrastructure.Meta
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public readonly struct SensoryResistance
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
    }
}