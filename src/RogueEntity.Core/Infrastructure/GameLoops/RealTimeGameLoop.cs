using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Infrastructure.GameLoops
{
    /// <summary>
    ///   This game loop keeps ticking even if the user does not perform any
    ///   actions. This behaviour is similar to Unity and other game engines.
    /// </summary>
    public class RealTimeGameLoop : ITimeSource, IGameLoop
    {
        /// <summary>
        ///   An optional time span defining how much simulated time passes on each fixed step.
        /// </summary>
        readonly Optional<TimeSpan> gameFixedTimeStep;

        readonly ILogger logger = SLog.ForContext<RealTimeGameLoop>();

        readonly GameTimeProcessor timeProcessor;
        
        TimeSpan fixedTimeUpdateTargetTime;
        TimeSpan fixedTimeUpdateHandledTime;

        public RealTimeGameLoop(int fps = 60, Optional<TimeSpan> gameFixedTimeStep = default)
        {
            this.gameFixedTimeStep = gameFixedTimeStep;
            timeProcessor = GameTimeProcessor.WithFramesPerSecond(fps);

            // commands = new Queue<Action>();
            PreFixedStepHandlers = new List<ActionSystemEntry>();
            FixedStepHandlers = new List<ActionSystemEntry>();
            LateFixedStepHandlers = new List<ActionSystemEntry>();
            VariableStepHandlers = new List<ActionSystemEntry>();
            LateVariableStepHandlers = new List<ActionSystemEntry>();
            InitializationStepHandlers = new List<ActionSystemEntry>();
            DisposeStepHandlers = new List<ActionSystemEntry>();
        }

        public ITimeSource TimeSource
        {
            get { return this; }
        }

        public bool IsRunning { get; private set; }

        public TimeSpan CurrentTime => TimeState.TotalGameTimeElapsed;

        public int FixedStepTime => TimeState.FixedStepCount;

        /// <summary>
        ///   A global set of handlers that runs once at the start of  each new game.
        ///   Use them to initialize the world after loading or generating content.
        /// </summary>
        public List<ActionSystemEntry> InitializationStepHandlers { get; }

        public List<ActionSystemEntry> DisposeStepHandlers { get; }

        public List<ActionSystemEntry> FixedStepHandlers { get; }

        public List<ActionSystemEntry> PreFixedStepHandlers { get; }

        public List<ActionSystemEntry> LateFixedStepHandlers { get; }

        public List<ActionSystemEntry> VariableStepHandlers { get; }

        public List<ActionSystemEntry> LateVariableStepHandlers { get; }

        public Func<bool> IsWaitingForInputDelegate { get; set; }

        public event EventHandler<WorldStepEventArgs> FixStepProgress;
        public event EventHandler<WorldStepEventArgs> VariableStepProgress;

        bool IsWaitingForInput()
        {
            return IsWaitingForInputDelegate?.Invoke() ?? true;
        }

        public void Initialize(Func<bool> isWaitingForInputDelegate = null)
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("GameLoop is already initialized");
            }

            IsRunning = true;
            IsWaitingForInputDelegate = isWaitingForInputDelegate;

            if (gameFixedTimeStep.TryGetValue(out var ts))
            {
                TimeState = new GameTimeState(ts);
            }
            else
            {
                TimeState = new GameTimeState(timeProcessor.TimeStepDuration);
            }

            fixedTimeUpdateTargetTime = TimeSpan.Zero;
            fixedTimeUpdateHandledTime = TimeSpan.Zero;

            foreach (var step in InitializationStepHandlers)
            {
                step.PerformAction(ActionSystemExecutionContext.Initialization);
            }
        }

        public void Stop()
        {
            if (!IsRunning) 
                return;
            
            IsRunning = false;
            for (var i = DisposeStepHandlers.Count - 1; i >= 0; i--)
            {
                var handler = DisposeStepHandlers[i];
                handler.PerformAction(ActionSystemExecutionContext.ShutDown);
            }

            IsWaitingForInputDelegate = null;
        }

        public void Dispose()
        {
            Stop();
        }

        [SuppressMessage("ReSharper", "InconsistentContextLogPropertyNaming")]
        public void Update(TimeSpan absoluteTime)
        {
            var w = Stopwatch.StartNew();
            logger.Verbose("Enter Update: {Time}", absoluteTime);
            // if (commands.Count > 0)
            // {
            //     var cmdGameContext = ContextProvider(TimeState.FrameDeltaTime, TimeState.TotalGameTimeElapsed);
            //     using (LogContext.PushProperty("GameLoop.Activity", "CommandProcessing"))
            //     {
            //         logger.Debug("Processing {CommandCount} commands.", commands.Count);
            //         while (commands.TryDequeue(out var command))
            //         {
            //             command(cmdGameContext);
            //         }
            //     }
            // }
            //

            if (!IsWaitingForInput())
            {
                var fixedSteps = timeProcessor.ComputeFixedStepCount(TimeState, absoluteTime,
                                                                     ref fixedTimeUpdateHandledTime,
                                                                     ref fixedTimeUpdateTargetTime);
                using (LogContext.PushProperty("GameLoop.Activity", "FixedTimeStep"))
                {
                    for (var timeStep = 0; timeStep < fixedSteps; timeStep += 1)
                    {
                        var w2 = Stopwatch.StartNew();

                        TimeState = GameTimeProcessor.NextFixedStep(TimeState);
                        foreach (var handler in PreFixedStepHandlers)
                        {
                            using (LogContext.PushProperty("GameLoop.Time", w2.Elapsed.TotalMilliseconds))
                            {
                                handler.PerformAction(ActionSystemExecutionContext.PrepareFixedStep);
                            }
                        }

                        FixStepProgress?.Invoke(this, new WorldStepEventArgs(TimeState));

                        foreach (var handler in FixedStepHandlers)
                        {
                            using (LogContext.PushProperty("GameLoop.Time", w2.Elapsed.TotalMilliseconds))
                            {
                                handler.PerformAction(ActionSystemExecutionContext.FixedStep);
                            }
                        }

                        foreach (var stepHandler in LateFixedStepHandlers)
                        {
                            using (LogContext.PushProperty("GameLoop.Time", w2.Elapsed.TotalMilliseconds))
                            {
                                stepHandler.PerformAction(ActionSystemExecutionContext.LateFixedStep);
                            }
                        }

                        logger.Information("Processed frame in {Elapsed} ({ElapsedTotalMilliseconds})", w2.Elapsed, w2.Elapsed.TotalMilliseconds);
                    }
                }
            }

            TimeState = timeProcessor.AdvanceFrameTimeOnly(TimeState, absoluteTime);

            using (LogContext.PushProperty("GameLoop.Activity", "VariableTimeStep"))
            {
                foreach (var stepHandler in VariableStepHandlers)
                {
                    using (LogContext.PushProperty("GameLoop.Time", w.Elapsed.TotalMilliseconds))
                    {
                        stepHandler.PerformAction(ActionSystemExecutionContext.VariableStep);
                    }
                }

                VariableStepProgress?.Invoke(this, new WorldStepEventArgs(TimeState));

                foreach (var stepHandler in LateVariableStepHandlers)
                {
                    using (LogContext.PushProperty("GameLoop.Time", w.Elapsed.TotalMilliseconds))
                    {
                        stepHandler.PerformAction(ActionSystemExecutionContext.LateVariableStep);
                    }
                }
            }

            logger.Verbose("Finished Update at {Elapsed} ({ElapsedTotalMilliseconds})", w.Elapsed, w.Elapsed.TotalMilliseconds);
        }

        public GameTimeState TimeState { get; private set; }
    }
}
