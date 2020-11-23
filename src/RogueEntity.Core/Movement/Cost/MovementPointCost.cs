using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;

namespace RogueEntity.Core.Movement.Cost
{
    /// <summary>
    ///   Encodes movement costs as an absolute cost per tile moved. Games like Heroes of Magic provide each unit with a budget of movement points,
    ///   and each terrain then has an associated movement point cost that is consumed for each tile the character moves.
    /// </summary>
    /// <typeparam name="TMovementMode"></typeparam>
    [EntityComponent]
    [MessagePackObject]
    [DataContract]
    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Discriminator")]
    public readonly struct MovementPointCost<TMovementMode>
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly float Cost;

        [SerializationConstructor]
        public MovementPointCost(float cost)
        {
            Cost = cost;
        }
    }
}