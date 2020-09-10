using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;

namespace RogueEntity.Core.Meta.ItemTraits
{
    [EntityComponent]
    [DataContract]
    [MessagePackObject]
    public readonly struct PlayerTag
    {
    }
}