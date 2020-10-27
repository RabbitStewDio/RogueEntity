using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;

namespace RogueEntity.Core.Sensing.Sources
{
    [EntityComponent(EntityConstructor.Flag)]
    [DataContract]
    [MessagePackObject]
    public readonly struct ObservedSenseSource<TSense> 
        where TSense: ISense
    {
        
    }
}