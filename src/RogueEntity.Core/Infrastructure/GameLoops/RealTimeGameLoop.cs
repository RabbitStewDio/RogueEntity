using EnTTSharp;
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
    ///   On each update a variable number of fixed-update steps will be
    ///   executed depending on the time passed since the last update.
    /// </summary>
    public class RealTimeGameLoop : ITimeSource, IGameLoop
    {
        /// <summary>
        ///   An optional initial value for how much fixed step time has
        ///   passed. Can be used to initialize the game loop to a previous
        ///   state.
        /// </summary>
        readonly Optional<TimeSpan> gameFixedTimeStep;

        readonly ILogger logger = SLog.ForContext<RealTimeGameLoop>();

        readonly GameTimeProcessor timeProcessor;

        TimeSpan fixedTimeUpdateTargetTime;
        TimeSpan fixedTimeUpdateHandledTime;

        public RealTimeGameLoop(ITimeSourceDefinition timeSourceDefinition, 
                                double fps = 60, 
                                Optional<TimeSpan> gameFixedTimeStep = default)
        {
            this.gameFixedTimeStep = gameFixedTimeStep;
            timeProcessor = GameTimeProcessor.WithFramesPerSecond(fps);

            TimeSourceDefinition = timeSourceDefinition ?? throw new ArgumentNullException(nameof(timeSourceDefinition));
            PreFixedStepHandlers = new List<ActionSystemEntry>();
            FixedStepHandlers = new List<ActionSystemEntry>();
            LateFixedStepHandlers = new List<ActionSystemEntry>();
            VariableStepHandlers = new List<ActionSystemEntry>();
            LateVariableStepHandlers = new List<ActionSystemEntry>();
            InitializationStepHandlers = new List<ActionSystemEntry>();
            DisposeStepHandlers = new List<ActionSystemEntry>();
        }

        public ITimeSourceDefinition TimeSourceDefinition { get; }

        public ITimeSource TimeSource
        {
            get { return this; }
        }

        public bool IsRunning { get; private set; }

        public double TicksPerSecond => timeProcessor.TicksPerSecond;
        
        public TimeSpan CurrentTime => timeState.TotalGameTimeElapsed;

        public int FixedStepFrameCounter => timeState.FixedStepCount;
        public TimeSpan FixedTimeStep => timeProcessor.TimeStepDuration;

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

        public Func<bool>? IsWaitingForInputDelegate { get; set; }

        public event EventHandler<WorldStepEventArgs>? FixStepProgress;
        public event EventHandler<WorldStepEventArgs>? VariableStepProgress;

        bool IsWaitingForInput()
        {
            return IsWaitingForInputDelegate?.Invoke() ?? true;
        }

        public void Initialize(Func<bool>? isWaitingForInputDelegate = null)
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("GameLoop is already initialized");
            }

            IsRunning = true;
            IsWaitingForInputDelegate = isWaitingForInputDelegate;

            if (gameFixedTimeStep.TryGetValue(out var ts))
            {
                timeState = new GameTimeState(ts);
            }
            else
            {
                timeState = new GameTimeState(timeProcessor.TimeStepDuration);
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

            IsWaitingForInputDelegate = default;
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
                var fixedSteps = timeProcessor.ComputeFixedStepCount(timeState, absoluteTime,
                                                                     ref fixedTimeUpdateHandledTime,
                                                                     ref fixedTimeUpdateTargetTime);
                using (LogContext.PushProperty("GameLoop.Activity", "FixedTimeStep"))
                {
                    for (var timeStep = 0; timeStep < fixedSteps; timeStep += 1)
                    {
                        var w2 = Stopwatch.StartNew();

                        timeState = GameTimeProcessor.NextFixedStep(timeState);
                        foreach (var handler in PreFixedStepHandlers)
                        {
                            using (LogContext.PushProperty("GameLoop.Time", w2.Elapsed.TotalMilliseconds))
                            {
                                handler.PerformAction(ActionSystemExecutionContext.PrepareFixedStep);
                            }
                        }

                        FixStepProgress?.Invoke(this, new WorldStepEventArgs(timeState));

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
            else
            {
                var fixedSteps = timeProcessor.ComputeFixedStepCount(timeState, absoluteTime,
                                                                     ref fixedTimeUpdateHandledTime,
                                                                     ref fixedTimeUpdateTargetTime);
                logger.Verbose("Skipping {FixedStepCount} fixed update frames due to waiting for input", fixedSteps);
            }

            timeState = timeProcessor.AdvanceFrameTimeOnly(timeState, absoluteTime);

            using (LogContext.PushProperty("GameLoop.Activity", "VariableTimeStep"))
            {
                foreach (var stepHandler in VariableStepHandlers)
                {
                    using (LogContext.PushProperty("GameLoop.Time", w.Elapsed.TotalMilliseconds))
                    {
                        stepHandler.PerformAction(ActionSystemExecutionContext.VariableStep);
                    }
                }

                VariableStepProgress?.Invoke(this, new WorldStepEventArgs(timeState));

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

        GameTimeState timeState;

        public ref readonly GameTimeState TimeState => ref timeState;
    }
}
