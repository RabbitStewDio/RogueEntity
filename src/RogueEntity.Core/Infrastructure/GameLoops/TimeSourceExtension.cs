using System;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Infrastructure.GameLoops
{
    public static class TimeSourceExtension
    {
        public static TimeSpan TurnToTime(this ITimeSource t, int turn)
        {
            return t.TimeState.FixedDeltaTime.Multiply(turn);
        }
    }
}