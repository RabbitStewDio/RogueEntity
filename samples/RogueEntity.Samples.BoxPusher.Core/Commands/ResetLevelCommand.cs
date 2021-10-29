using EnTTSharp.Entities.Attributes;
using MessagePack;
using System;
using System.Runtime.Serialization;

namespace RogueEntity.Samples.BoxPusher.Core.Commands
{
    [EntityComponent(EntityConstructor.Flag)]
    [MessagePackObject]
    [Serializable]
    [DataContract]
    public readonly struct ResetLevelCommand
    {
    }
}
