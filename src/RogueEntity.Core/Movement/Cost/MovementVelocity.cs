using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;

namespace RogueEntity.Core.Movement.Cost
{
    /// <summary>
    ///   A velocity expresses distance an actor can move per fixed unit of time. This
    ///   is the inverse metric of movement points. Increasing the velocity reduces the
    ///   time requirement for moving one unit of distance (for instance one grid field).
    ///
    ///   By convention, this velocity should be expressed as movement per tick. Use
    ///   an <see cref="RogueEntity.Api.Time.ITimeSourceDefinition"/> to resolve it to
    ///   ticks where needed. 
    ///
    ///   Velocity has a natural upper limit of 1 turn per move in turn-based grid games.
    ///   Thus, if you do use velocity as a measure of movement cost, make sure that you
    ///   apply a suitable scale to all calculations.
    ///
    ///   When assuming a scale of 1 a velocity of 1 means a character moves one grid square
    ///   per turn; a velocity of 5 means a unit could cross 5 squares per turn while a
    ///   velocity of 0.5 means a unit will take two turns to cross a tile.  
    /// </summary>
    /// <typeparam name="TMovementMode"></typeparam>
    [EntityComponent]
    [MessagePackObject]
    [DataContract]
    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Discriminiator")]
    public readonly struct MovementVelocity<TMovementMode>
    {
        /// <summary>
        ///   Velocity is usually stored as movement units (meters) per game tick.
        /// </summary>
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
