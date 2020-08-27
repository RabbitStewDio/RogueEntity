using System.Runtime.Serialization;
using EnTTSharp.Annotations;
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