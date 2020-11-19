using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Movement.Cost
{
    /// <summary>
    ///   Describes the inverse of movement cost.
    /// </summary>
    /// <typeparam name="TMovementMode"></typeparam>
    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Discriminiator")]
    public readonly struct MovementVelocity<TMovementMode>
    {
        public readonly float Velocity;

        public MovementVelocity(float velocity)
        {
            Velocity = velocity;
        }
    }
}