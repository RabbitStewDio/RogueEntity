using System;

namespace RogueEntity.Api.Time
{
    public interface ITimeSource
    {
        TimeSpan CurrentTime{ get; }
        int FixedStepTime { get; }
        GameTimeState TimeState { get; }
    }
}