using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing
{
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public readonly struct DiscoveryMapData
    {
        [Key(0)]
        [DataMember(Order = 0)]
        public readonly PackedBoolMap Map;

        [SerializationConstructor]
        public DiscoveryMapData(int width, int height)
        {
            Map = new PackedBoolMap(width, height);
        }
    }
}