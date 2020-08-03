using System;
using System.Runtime.Serialization;
using EnTTSharp.Annotations;
using MessagePack;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Inventory
{
    [EntityComponent]
    [Serializable]
    [DataContract]
    [MessagePackObject]
    public readonly struct ContainedInInventoryMarker<TOwnerId, TItemId>
    {
        [DataMember]
        [Key(0)]
        public readonly Optional<TOwnerId> Owner;

        [DataMember]
        [Key(1)]
        public readonly Optional<TItemId> Container;

        public ContainedInInventoryMarker(TOwnerId owner)
        {
            Owner = owner;
            Container = Optional.Empty<TItemId>();
        }

        [SerializationConstructor]
        public ContainedInInventoryMarker(TOwnerId owner, TItemId container)
        {
            Owner = owner;
            Container = container;
        }

        public bool IsUnowned()
        {
            return !Owner.HasValue;
        }
    }
}