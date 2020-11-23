using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;

namespace RogueEntity.Core.Movement.Cost
{
    /// <summary>
    ///   Describes the inverse of movement cost.
    /// </summary>
    /// <typeparam name="TMovementMode"></typeparam>
    [EntityComponent]
    [MessagePackObject]
    [DataContract]
    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Discriminiator")]
    public readonly struct MovementVelocity<TMovementMode>
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly float Velocity;

        [SerializationConstructor]
        public MovementVelocity(float velocity)
        {
            Velocity = velocity;
        }
    }
}