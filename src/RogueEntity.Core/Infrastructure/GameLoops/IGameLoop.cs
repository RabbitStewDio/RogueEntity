using System;
using System.Collections.Generic;
using RogueEntity.Core.Infrastructure.Time;

namespace RogueEntity.Core.Infrastructure.GameLoops
{
    public delegate TGameContext ContextForTimeStepProvider<out TGameContext>(TimeSpan elapsedTime, TimeSpan totalTime);

    public delegate void WorldStepDelegate<in TGameContext>(TGameContext context, GameTimeState time);

    /// <summary>
    ///  This interface is intentionally kept separate from the normal game loop interface so that
    ///  nosy people don't get tempted to use it for their own purposes. The late step handlers contain
    ///  clean up code that must run each frame.
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    public interface ISystemGameLoop<TGameContext>
    {
        List<ActionSystemEntry<TGameContext>> LateStepHandlers { get; }
    }

    public interface ISystemGameLoopRegistration<TGameContext>
    {
        void AddLateStepHandlers(Action<TGameContext> c);
    }

    public interface IGameLoopSystemRegistration<TGameContext>
    {
        TGameContext Context { get; }
        void AddInitializationStepHandler(Action<TGameContext> c);
        void AddPreFixedStepHandlers(Action<TGameContext> c);
        void AddFixedStepHandlers(Action<TGameContext> c);
        void AddVariableStepHandlers(Action<TGameContext> c);
        void AddDisposeStepHandler(Action<TGameContext> c);
    }

    public interface IGameLoop<TGameContext>
    {
        void Enqueue(Action<TGameContext> command);
        void Initialize();
        void Update(TimeSpan absoluteTime);
        ITimeSource TimeSource { get; }
    }
}