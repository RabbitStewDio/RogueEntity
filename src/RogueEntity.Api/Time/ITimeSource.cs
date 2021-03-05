using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Utils;
using System;

namespace RogueEntity.Api.Time
{
    public interface ITimeSource
    {
        TimeSpan CurrentTime{ get; }
        int FixedStepTime { get; }
        ref readonly GameTimeState TimeState { get; }
    }

    public interface ITimeSourceDefinition
    {
        double TicksPerSecond { get; }
        IGameLoop BuildTimeStepLoop(IGameLoopSystemInformation t, Optional<TimeSpan> fixedStepTime = default);
    }
}