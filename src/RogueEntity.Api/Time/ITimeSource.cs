using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Modules;
using System;

namespace RogueEntity.Api.Time
{
    public interface ITimeSource
    {
        ITimeSourceDefinition TimeSourceDefinition { get; }
        TimeSpan CurrentTime{ get; }
        int FixedStepTime { get; }
        ref readonly GameTimeState TimeState { get; }
    }

    public interface ITimeSourceDefinition
    {
        double TicksPerSecond { get; }
        IGameLoop BuildTimeStepLoop(IGameLoopSystemInformation t);
    }
}