using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using Serilog;
using Serilog.Context;

namespace RogueEntity.Api.GameLoops
{
    /// <summary>
    ///   This game loop keeps ticking even if the user does not perform any
    ///   actions. This behaviour is similar to Unity and other game engines.
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    public class RealTimeGameLoop<TGameContext> : ITimeSource, IGameLoop<TGameContext>, IDisposable
    {
        /// <summary>
        ///   An optional time span defining how much simulated time passes on each fixed step.
        /// </summary>
        readonly Optional<TimeSpan> gameFixedTimeStep;

        readonly ILogger logger = SLog.ForContext<RealTimeGameLoop<TGameContext>>();

        readonly GameTimeProcessor timeProcessor;
        // readonly Queue<Action<TGameContext>> commands;

        public RealTimeGameLoop(int fps = 60, Optional<TimeSpan> gameFixedTimeStep = default)
        {
            this.gameFixedTimeStep = gameFixedTimeStep;
            timeProcessor = GameTimeProcessor.WithFramesPerSecond(fps);

            // commands = new Queue<Action<TGameContext>>();
            PreFixedStepHandlers = new List<ActionSystemEntry<TGameContext>>();
            FixedStepHandlers = new List<ActionSystemEntry<TGameContext>>();
            LateFixedStepHandlers = new List<ActionSystemEntry<TGameContext>>();
            VariableStepHandlers = new List<ActionSystemEntry<TGameContext>>();
            LateVariableStepHandlers = new List<ActionSystemEntry<TGameContext>>();
            InitializationStepHandlers = new List<ActionSystemEntry<TGameContext>>();
            DisposeStepHandlers = new List<ActionSystemEntry<TGameContext>>();
        }

        public ITimeSource TimeSource
        {
            get { return this; }
        }

        public TimeSpan CurrentTime => TimeState.TotalGameTimeElapsed;

        public int FixedStepTime => TimeState.FixedStepCount;

        /// <summary>
        ///   A global set of handlers that runs once at the start of  each new game.
        ///   Use them to initialize the world after loading or generating content.
        /// </summary>
        public List<ActionSystemEntry<TGameContext>> InitializationStepHandlers { get; }

        public List<ActionSystemEntry<TGameContext>> DisposeStepHandlers { get; }

        public List<ActionSystemEntry<TGameContext>> FixedStepHandlers { get; }

        public List<ActionSystemEntry<TGameContext>> PreFixedStepHandlers { get; }

        public List<ActionSystemEntry<TGameContext>> LateFixedStepHandlers { get; }
        
        public List<ActionSystemEntry<TGameContext>> VariableStepHandlers { get; }

        public List<ActionSystemEntry<TGameContext>> LateVariableStepHandlers { get; }

        public Func<bool> IsWaitingForInputDelegate { get; set; }
        public ContextForTimeStepProvider<TGameContext> ContextProvider { get; set; }

        public event EventHandler<WorldStepEventArgs<TGameContext>> FixStepProgress;
        public event EventHandler<WorldStepEventArgs<TGameContext>> VariableStepProgress;

        bool IsWaitingForInput()
        {
            return IsWaitingForInputDelegate?.Invoke() ?? true;
        }

        public void Initialize([NotNull] ContextForTimeStepProvider<TGameContext> contextProvider, Func<bool> isWaitingForInputDelegate = null)
        {
            ContextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
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

            var context = ContextProvider(TimeSpan.Zero, TimeState.FixedGameTimeElapsed);
            foreach (var step in InitializationStepHandlers)
            {
                step.PerformAction(context, ActionSystemExecutionContext.Initialization);
            }
        }

        public void Dispose()
        {
            if (ContextProvider == null)
            {
                return;
            }
            
            var context = ContextProvider(TimeState.FrameDeltaTime, TimeState.FixedGameTimeElapsed);
            for (var i = DisposeStepHandlers.Count - 1; i >= 0; i--)
            {
                var handler = DisposeStepHandlers[i];
                handler.PerformAction(context, ActionSystemExecutionContext.ShutDown);
            }
            
            ContextProvider = null;
            IsWaitingForInputDelegate = null;
        }

        TimeSpan fixedTimeUpdateTargetTime;
        TimeSpan fixedTimeUpdateHandledTime;

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
                        var fixedStepContext = ContextProvider(TimeState.FixedDeltaTime, TimeState.FixedGameTimeElapsed);
                        foreach (var handler in PreFixedStepHandlers)
                        {
                            using (LogContext.PushProperty("GameLoop.Time", w2.Elapsed.TotalMilliseconds))
                            {
                                handler.PerformAction(fixedStepContext, ActionSystemExecutionContext.PrepareFixedStep);
                            }
                        }

                        FixStepProgress?.Invoke(this, new WorldStepEventArgs<TGameContext>(fixedStepContext, TimeState));

                        foreach (var handler in FixedStepHandlers)
                        {
                            using (LogContext.PushProperty("GameLoop.Time", w2.Elapsed.TotalMilliseconds))
                            {
                                handler.PerformAction(fixedStepContext, ActionSystemExecutionContext.FixedStep);
                            }
                        }

                        foreach (var stepHandler in LateFixedStepHandlers)
                        {
                            using (LogContext.PushProperty("GameLoop.Time", w2.Elapsed.TotalMilliseconds))
                            {
                                stepHandler.PerformAction(fixedStepContext, ActionSystemExecutionContext.LateFixedStep);
                            }
                        }

                        logger.Information("Processed frame in {Elapsed} ({ElapsedTotalMilliseconds})", w2.Elapsed, w2.Elapsed.TotalMilliseconds);
                    }
                }
            }

            TimeState = timeProcessor.AdvanceFrameTimeOnly(TimeState, absoluteTime);

            using (LogContext.PushProperty("GameLoop.Activity", "VariableTimeStep"))
            {
                var gameContext = ContextProvider(TimeState.FrameDeltaTime, TimeState.TotalGameTimeElapsed);
                foreach (var stepHandler in VariableStepHandlers)
                {
                    using (LogContext.PushProperty("GameLoop.Time", w.Elapsed.TotalMilliseconds))
                    {
                        stepHandler.PerformAction(gameContext, ActionSystemExecutionContext.VariableStep);
                    }
                }

                VariableStepProgress?.Invoke(this, new WorldStepEventArgs<TGameContext>(gameContext, TimeState));
                
                foreach (var stepHandler in LateVariableStepHandlers)
                {
                    using (LogContext.PushProperty("GameLoop.Time", w.Elapsed.TotalMilliseconds))
                    {
                        stepHandler.PerformAction(gameContext, ActionSystemExecutionContext.LateVariableStep);
                    }
                }
            }

            logger.Verbose("Finished Update at {Elapsed} ({ElapsedTotalMilliseconds})", w.Elapsed, w.Elapsed.TotalMilliseconds);
        }

        public GameTimeState TimeState { get; private set; }
    }
}