using System.Runtime.Serialization;
using EnTTSharp.Annotations;
using MessagePack;

namespace RogueEntity.Core.Infrastructure.Meta
{
    [EntityComponent]
    [DataContract]
    [MessagePackObject]
    public readonly struct PlayerTag
    {
    }
}