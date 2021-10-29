using EnTTSharp.Entities.Attributes;
using MessagePack;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Players
{
    [EntityComponent(EntityConstructor.Flag)]
    [MessagePackObject]
    [DataContract]
    public readonly struct NewPlayerSpawnRequest
    {
    }
}
