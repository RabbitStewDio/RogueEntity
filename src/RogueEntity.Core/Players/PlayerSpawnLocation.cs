using EnTTSharp.Entities.Attributes;
using MessagePack;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Players
{
    [EntityComponent()]
    [MessagePackObject]
    [DataContract]
    public readonly struct PlayerSpawnLocation
    {
    }
}
