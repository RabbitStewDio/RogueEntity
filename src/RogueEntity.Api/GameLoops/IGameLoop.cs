using System;
using RogueEntity.Api.Time;

namespace RogueEntity.Api.GameLoops
{
    public delegate TGameContext ContextForTimeStepProvider<out TGameContext>(TimeSpan elapsedTime, TimeSpan totalTime);

    public readonly struct WorldStepEventArgs<TGameContext>
    {
        public readonly TGameContext Context;
        public readonly GameTimeState Time;

        public WorldStepEventArgs(TGameContext context, GameTimeState time)
        {
            Context = context;
            Time = time;
        }
    } 
    
    public interface IGameLoopSystemRegistration<TGameContext>
    {
        // TGameContext Context { get; }
        void AddInitializationStepHandler(Action<TGameContext> c, string description = null);
        void AddPreFixedStepHandlers(Action<TGameContext> c, string description = null);
        void AddFixedStepHandlers(Action<TGameContext> c, string description = null);
        void AddLateFixedStepHandlers(Action<TGameContext> c, string description = null);
        void AddVariableStepHandlers(Action<TGameContext> c, string description = null);
        void AddLateVariableStepHandlers(Action<TGameContext> c, string description = null);
        void AddDisposeStepHandler(Action<TGameContext> c, string description = null);
    }

    public interface IGameLoop<TGameContext>
    {
        void Initialize(ContextForTimeStepProvider<TGameContext> contextProvider, 
                        Func<bool> isWaitingForInputDelegate = null);
        void Update(TimeSpan absoluteTime);
        ITimeSource TimeSource { get; }

        event EventHandler<WorldStepEventArgs<TGameContext>> FixStepProgress;
        event EventHandler<WorldStepEventArgs<TGameContext>> VariableStepProgress;
    }
}