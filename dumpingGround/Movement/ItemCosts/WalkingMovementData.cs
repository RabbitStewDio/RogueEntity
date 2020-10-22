using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;

namespace RogueEntity.Core.Movement.ItemCosts
{
    [EntityComponent]
    [MessagePackObject]
    [DataContract]
    public readonly struct WalkingMovementData
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly MovementCost Cost;

        [SerializationConstructor]
        public WalkingMovementData(MovementCost cost)
        {
            this.Cost = cost;
        }
    }
}