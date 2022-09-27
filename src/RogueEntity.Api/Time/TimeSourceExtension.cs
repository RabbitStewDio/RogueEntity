using System;

namespace RogueEntity.Api.Time
{
    public static class TimeSourceExtension
    {
        public static TimeSpan TurnToTime(this ITimeSource t, int turn)
        {
            return t.TimeState.FixedDeltaTime.Multiply(turn);
        }
    }
}