using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Utils;
using Serilog;
using Serilog.Context;

namespace RogueEntity.Core.Infrastructure.GameLoops
{
    /// <summary>
    ///    A game loop with simulated time that stops when any player's turn is active.
    ///    This loop always advances by a fixed number of time steps.
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    public class FixedTimeStepGameLoop<TGameContext> : ITimeSource, 
                                                       IGameLoop<TGameContext>, 
                                                       IDisposable
    {
        readonly ILogger logger = SLog.ForContext<FixedTimeStepGameLoop<TGameContext>>();

        readonly int maxFixStepsPerUpdate;
        readonly Queue<Action<TGameContext>> commands;
        readonly GameTimeProcessor timeProcessor;

        public FixedTimeStepGameLoop(int maxFixStepsPerUpdate, TimeSpan fixedDeltaTime)
        {
            this.maxFixStepsPerUpdate = maxFixStepsPerUpdate;
            timeProcessor = new GameTimeProcessor(fixedDeltaTime);

            commands = new Queue<Action<TGameContext>>();
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

        public List<ActionSystemEntry<TGameContext>> PreFixedStepHandlers { get; }

        public List<ActionSystemEntry<TGameContext>> FixedStepHandlers { get; }
        
        public List<ActionSystemEntry<TGameContext>> LateFixedStepHandlers { get; }

        public List<ActionSystemEntry<TGameContext>> VariableStepHandlers { get; }

        public List<ActionSystemEntry<TGameContext>> LateVariableStepHandlers { get; }

        public List<ActionSystemEntry<TGameContext>> DisposeStepHandlers { get; }

        public ContextForTimeStepProvider<TGameContext> ContextProvider { get; set; }
        
        public Func<bool> IsWaitingForInputDelegate { get; set; }

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
            
            TimeState = new GameTimeState(this.timeProcessor.TimeStepDuration);

            var context = ContextProvider(TimeSpan.Zero, TimeState.FixedGameTimeElapsed);
            foreach (var step in InitializationStepHandlers)
            {
                step.PerformAction(context, ActionSystemExecutionContext.Initialization);
            }
        }

        public void Dispose()
        {
            var context = ContextProvider(TimeState.FrameDeltaTime, TimeState.FixedGameTimeElapsed);
            for (var i = DisposeStepHandlers.Count - 1; i >= 0; i--)
            {
                var handler = DisposeStepHandlers[i];
                handler.PerformAction(context, ActionSystemExecutionContext.ShutDown);
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

                        logger.Verbose("Processed frame in {Elapsed} ({ElapsedTotalMilliseconds})", w2.Elapsed, w2.Elapsed.TotalMilliseconds);
                    } while (!IsWaitingForInput() && count < maxFixStepsPerUpdate);

                    logger.Information("Processed {Count} events at {Elapsed} ({ElapsedTotalMilliseconds};{FixedTimeStep})",
                                       count, w.Elapsed, w.Elapsed.TotalMilliseconds, TimeState.FixedStepCount);
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

        public void Enqueue(Action<TGameContext> command)
        {
            commands.Enqueue(command);
        }
    }
}