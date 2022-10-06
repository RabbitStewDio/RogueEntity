using System;
using RogueEntity.Api.Time;

namespace RogueEntity.Api.GameLoops
{
    public interface IGameLoop: IDisposable
    {
        void Initialize(Func<bool>? isWaitingForInputDelegate = null);
        void Update(TimeSpan absoluteTime);
        void Stop();
        
        bool IsRunning { get; }
        ITimeSource TimeSource { get; }
        
        event EventHandler<WorldStepEventArgs> FixStepProgress;
        event EventHandler<WorldStepEventArgs> VariableStepProgress;
    }
}