using EnTTSharp.Entities.Attributes;
using MessagePack;
using System.Runtime.Serialization;

namespace RogueEntity.Core.MapLoading.PlayerSpawning
{
    [EntityComponent()]
    [MessagePackObject]
    [DataContract]
    public readonly struct PlayerSpawnLocation
    {
    }
}
