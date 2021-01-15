using System;
using RogueEntity.Api.Time;

namespace RogueEntity.Api.GameLoops
{
    public readonly struct WorldStepEventArgs
    {
        public readonly GameTimeState Time;

        public WorldStepEventArgs(GameTimeState time)
        {
            Time = time;
        }
    } 
    
    public interface IGameLoopSystemRegistration
    {
        void AddInitializationStepHandler(Action c, string description = null);
        void AddPreFixedStepHandlers(Action c, string description = null);
        void AddFixedStepHandlers(Action c, string description = null);
        void AddLateFixedStepHandlers(Action c, string description = null);
        void AddVariableStepHandlers(Action c, string description = null);
        void AddLateVariableStepHandlers(Action c, string description = null);
        void AddDisposeStepHandler(Action c, string description = null);
    }

    public interface IGameLoop
    {
        void Initialize(Func<bool> isWaitingForInputDelegate = null);
        void Update(TimeSpan absoluteTime);
        ITimeSource TimeSource { get; }

        event EventHandler<WorldStepEventArgs> FixStepProgress;
        event EventHandler<WorldStepEventArgs> VariableStepProgress;
    }
}