using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;

namespace RogueEntity.Core.Meta.Base
{
    [EntityComponent]
    [Serializable]
    [DataContract]
    [MessagePackObject]
    public readonly struct ContainerEntityMarker<TOwnerId>: IContainerEntityMarker
    {
        [DataMember]
        [Key(0)]
        public readonly TOwnerId Owner;

        [SerializationConstructor]
        public ContainerEntityMarker(TOwnerId owner)
        {
            Owner = owner;
        }
    }
}