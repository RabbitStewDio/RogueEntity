using System;

namespace RogueEntity.Core.Infrastructure.Time
{
    public interface ITimeSource
    {
        TimeSpan CurrentTime{ get; }
        int FixedStepTime { get; }
        GameTimeState TimeState { get; }
    }
}