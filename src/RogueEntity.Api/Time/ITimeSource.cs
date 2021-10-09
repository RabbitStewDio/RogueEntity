using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Modules;
using System;

namespace RogueEntity.Api.Time
{
    public interface ITimeSource
    {
        ITimeSourceDefinition TimeSourceDefinition { get; }
        
        TimeSpan CurrentTime{ get; }
        
        /// <summary>
        ///   The time increment for each fixed-update processing step. 
        /// </summary>
        TimeSpan FixedTimeStep { get; }
        
        /// <summary>
        ///   A counter of fixed steps that have passed.
        /// </summary>
        int FixedStepFrameCounter { get; }
        
        ref readonly GameTimeState TimeState { get; }
    }

    public interface ITimeSourceDefinition
    {
        /// <summary>
        ///   The target framerate for the fixed update steps. 
        /// </summary>
        double UpdateTicksPerSecond { get; }
        
        IGameLoop BuildTimeStepLoop(IGameLoopSystemInformation t);
    }
}