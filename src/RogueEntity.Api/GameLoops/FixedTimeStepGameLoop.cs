﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using Serilog;
using Serilog.Context;

namespace RogueEntity.Api.GameLoops
{
    /// <summary>
    ///    A game loop with simulated time that stops when any player's turn is active.
    ///    This loop always advances by a fixed number of time steps.
    /// </summary>
    public class FixedTimeStepGameLoop : ITimeSource,
                                         IGameLoop,
                                         IDisposable
    {
        readonly ILogger logger = SLog.ForContext<FixedTimeStepGameLoop>();

        readonly int maxFixStepsPerUpdate;
        readonly Queue<Action> commands;
        readonly GameTimeProcessor timeProcessor;

        public FixedTimeStepGameLoop(int maxFixStepsPerUpdate, TimeSpan fixedDeltaTime)
        {
            this.maxFixStepsPerUpdate = maxFixStepsPerUpdate;
            timeProcessor = new GameTimeProcessor(fixedDeltaTime);

            commands = new Queue<Action>();
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

        public TimeSpan CurrentTime => TimeState.TotalGameTimeElapsed;
        public int FixedStepTime => TimeState.FixedStepCount;


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
            IsWaitingForInputDelegate = isWaitingForInputDelegate;

            TimeState = new GameTimeState(this.timeProcessor.TimeStepDuration);

            foreach (var step in InitializationStepHandlers)
            {
                step.PerformAction(ActionSystemExecutionContext.Initialization);
            }
        }

        public void Dispose()
        {
            for (var i = DisposeStepHandlers.Count - 1; i >= 0; i--)
            {
                var handler = DisposeStepHandlers[i];
                handler.PerformAction(ActionSystemExecutionContext.ShutDown);
            }
        }

        public void Update(TimeSpan absoluteTime)
        {
            var w = Stopwatch.StartNew();
            logger.Verbose("Enter Update: {Time} ({FixedFrame})", absoluteTime, TimeState.FixedStepCount);

            // if (commands.Count > 0)
            // {
            //     // Translates incoming commands into Entity system components. 
            //     // 
            //     // Command requests can come in at any time. We deliberately do that processing
            //     // only at this particular point, where we can guarantee that no one else is
            //     // modifying the entity system.
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

            if (!IsWaitingForInput())
            {
                using (LogContext.PushProperty("GameLoop.Activity", "FixedTimeStep"))
                {
                    int count = 0;
                    do
                    {
                        var w2 = Stopwatch.StartNew();
                        count += 1;
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

                        logger.Verbose("Processed frame in {Elapsed} ({ElapsedTotalMilliseconds})", w2.Elapsed, w2.Elapsed.TotalMilliseconds);
                    } while (!IsWaitingForInput() && count < maxFixStepsPerUpdate);

                    logger.Information("Processed {Count} events at {Elapsed} ({ElapsedTotalMilliseconds};{FixedTimeStep})",
                                       count, w.Elapsed, w.Elapsed.TotalMilliseconds, TimeState.FixedStepCount);
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

        public void Enqueue(Action command)
        {
            commands.Enqueue(command);
        }
    }
}
