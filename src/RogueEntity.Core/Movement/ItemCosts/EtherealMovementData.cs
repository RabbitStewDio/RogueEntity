using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;

namespace RogueEntity.Core.Movement.ItemCosts
{
    [EntityComponent]
    [MessagePackObject]
    [DataContract]
    public readonly struct EtherealMovementData
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly MovementCost Cost;

        [SerializationConstructor]
        public EtherealMovementData(MovementCost movementCost)
        {
            this.Cost = movementCost;
        }
    }
}