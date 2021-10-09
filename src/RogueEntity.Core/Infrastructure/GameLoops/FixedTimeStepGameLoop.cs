using JetBrains.Annotations;
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
    ///    A game loop with simulated time that stops when any player's turn is active.
    ///    This loop always advances by a fixed number of time steps. This means the
    ///    simulation will speed up or slow down depending on the frequency of the
    ///    incoming update calls.
    /// </summary>
    public class FixedTimeStepGameLoop : ITimeSource,
                                         IGameLoop
    {
        readonly ILogger logger = SLog.ForContext<FixedTimeStepGameLoop>();

        readonly int maxFixStepsPerUpdate;
        readonly Optional<TimeSpan> gameFixedTimeStep;
        readonly GameTimeProcessor timeProcessor;
        GameTimeState timeState;

        public FixedTimeStepGameLoop([NotNull] ITimeSourceDefinition timeSourceDefinition, 
                                     int maxFixStepsPerUpdate, 
                                     TimeSpan fixedDeltaTime, 
                                     Optional<TimeSpan> gameFixedTimeStep = default)
        {
            this.maxFixStepsPerUpdate = maxFixStepsPerUpdate;
            this.gameFixedTimeStep = gameFixedTimeStep;
            timeProcessor = new GameTimeProcessor(fixedDeltaTime);

            TimeSourceDefinition = timeSourceDefinition ?? throw new ArgumentNullException(nameof(timeSourceDefinition));
            PreFixedStepHandlers = new List<ActionSystemEntry>();
            FixedStepHandlers = new List<ActionSystemEntry>();
            LateFixedStepHandlers = new List<ActionSystemEntry>();
            VariableStepHandlers = new List<ActionSystemEntry>();
            LateVariableStepHandlers = new List<ActionSystemEntry>();
            InitializationStepHandlers = new List<ActionSystemEntry>();
            DisposeStepHandlers = new List<ActionSystemEntry>();
        }

        public TimeSpan FixedTimeStep => timeProcessor.TimeStepDuration;

        public ITimeSource TimeSource => this;

        public ITimeSourceDefinition TimeSourceDefinition { get; }

        public double TicksPerSecond => timeProcessor.TicksPerSecond;
        public TimeSpan CurrentTime => timeState.TotalGameTimeElapsed;
        public int FixedStepFrameCounter => timeState.FixedStepCount;

        public bool IsRunning { get; private set; }

        /// <summary>
        ///   A global set of handlers that runs once at the start of  each new game.
        ///   Use them to initialize the world after loading or generating content.
        /// </summary>
        public List<ActionSystemEntry> InitializationStepHandlers { get; }

        public List<ActionSystemEntry> PreFixedStepHandlers { get; }

        public List<ActionSystemEntry> FixedStepHandlers { get; }

        public List<ActionSystemEntry> LateFixedStepHandlers { get; }

        public List<ActionSystemEntry> VariableStepHandlers { get; }

        public List<ActionSystemEntry> LateVariableStepHandlers { get; }

        public List<ActionSystemEntry> DisposeStepHandlers { get; }

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
                throw new InvalidOperationException("GameLoop already running");
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

            foreach (var step in InitializationStepHandlers)
            {
                step.PerformAction(ActionSystemExecutionContext.Initialization);
            }
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }
            
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
            logger.Verbose("Enter Update: {Time} ({FixedFrame})", absoluteTime, TimeState.FixedStepCount);

            if (!IsWaitingForInput())
            {
                using (LogContext.PushProperty("GameLoop.Activity", "FixedTimeStep"))
                {
                    int count = 0;
                    do
                    {
                        var w2 = Stopwatch.StartNew();
                        count += 1;
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

                        logger.Verbose("Processed frame in {Elapsed} ({ElapsedTotalMilliseconds})", w2.Elapsed, w2.Elapsed.TotalMilliseconds);
                    } while (!IsWaitingForInput() && count < maxFixStepsPerUpdate);

                    logger.Information("Processed {Count} events at {Elapsed} ({ElapsedTotalMilliseconds};{FixedTimeStep})",
                                       count, w.Elapsed, w.Elapsed.TotalMilliseconds, timeState.FixedStepCount);
                }
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

        public ref readonly GameTimeState TimeState
        {
            get => ref timeState;
        }
    }
}
