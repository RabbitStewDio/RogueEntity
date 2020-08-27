using System;
using System.Collections.Generic;
using RogueEntity.Core.Infrastructure.Time;

namespace RogueEntity.Core.Movement.Pathing
{
    /// <summary>
    ///   Represents the intent to occupy a given cell soon.
    /// 
    ///   This claim is made as soon as the movement decision has been made by the actor
    ///   (usually when the movement is scheduled). Other actors can check the claim for
    ///   the cell they are currently occupying to see whether they should move to an
    ///   empty cell elsewhere.
    /// </summary>
    public readonly struct MovementIntent<TActorId>
    {
        static readonly IEqualityComparer<TActorId> EqualityComparer = EqualityComparer<TActorId>.Default;
        public readonly TActorId Source;
        public readonly int ClaimExpiryTime;

        public MovementIntent(TActorId source, int claimExpiryTime)
        {
            Source = source;
            ClaimExpiryTime = claimExpiryTime;
        }

        public bool IsExpired<TGameContext>(TGameContext c)
            where TGameContext: ITimeContext
        {
            return c.TimeSource.FixedStepTime > ClaimExpiryTime;
        }

        public bool IsValidMoveTarget<TGameContext>(TGameContext c, TActorId who)
            where TGameContext : ITimeContext
        {
            if (EqualityComparer.Equals(Source, who) || EqualityComparer.Equals(Source, default))
            {
                return true;
            }
            return (c.TimeSource.FixedStepTime > ClaimExpiryTime);
        }

        public bool IsValidMoveTarget<TGameContext>(TGameContext c, TActorId who, out int reservationTimeLeft)
            where TGameContext : ITimeContext
        {
            if (EqualityComparer.Equals(Source, who) || EqualityComparer.Equals(Source, default))
            {
                reservationTimeLeft = 0;
                return true;
            }

            reservationTimeLeft = Math.Max(0, ClaimExpiryTime - c.TimeSource.FixedStepTime);
            return reservationTimeLeft == 0;
        }
    }
}