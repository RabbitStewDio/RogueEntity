using System;

namespace RogueEntity.Core.Infrastructure.GameLoops
{
    public interface ITimeSource
    {
        TimeSpan CurrentTime{ get; }
        int FixedStepTime { get; }
        GameTimeState TimeState { get; }
    }
}