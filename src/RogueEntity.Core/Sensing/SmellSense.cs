using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Sensing
{
    [DataContract]
    [MessagePackObject]
    public readonly struct SmellSense: ISense
    {
    }
}