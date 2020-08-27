using System;
using System.Collections.Generic;
using System.Diagnostics;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Utils;
using Serilog;
using Serilog.Context;

namespace RogueEntity.Core.Infrastructure.GameLoops
{
    /// <summary>
    ///    A game loop with simulated time that stops when any player's turn is
    ///    active.
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    public class FixedTimeStepGameLoop<TGameContext> : ITimeSource, IGameLoop<TGameContext>, ISystemGameLoop<TGameContext>
    {
        readonly ILogger logger = SLog.ForContext<FixedTimeStepGameLoop<TGameContext>>();

        readonly int budget;
        readonly Queue<Action<TGameContext>> commands;
        readonly GameTimeProcessor timeProcessor;

        public FixedTimeStepGameLoop(int budget, TimeSpan fixedDeltaTime)
        {
            this.budget = budget;
            timeProcessor = new GameTimeProcessor(fixedDeltaTime);

            commands = new Queue<Action<TGameContext>>();
            PreFixedStepHandlers = new List<ActionSystemEntry<TGameContext>>();
            FixedStepHandlers = new List<ActionSystemEntry<TGameContext>>();
            VariableStepHandlers = new List<ActionSystemEntry<TGameContext>>();
            LateStepHandlers = new List<ActionSystemEntry<TGameContext>>();
            InitializationStepHandlers = new List<ActionSystemEntry<TGameContext>>();
        }

        public ITimeSource TimeSource
        {
            get { return this; }
        }

        public TimeSpan CurrentTime => TimeState.TotalGameTimeElapsed;
        public int FixedStepTime => TimeState.FixedStepCount;

        public List<ActionSystemEntry<TGameContext>> FixedStepHandlers { get; }

        /// <summary>
        ///   A global set of handlers that runs once at the start of  each new game.
        ///   Use them to initialize the world after loading or generating content.
        /// </summary>
        public List<ActionSystemEntry<TGameContext>> InitializationStepHandlers { get; }

        public List<ActionSystemEntry<TGameContext>> PreFixedStepHandlers { get; }

        public List<ActionSystemEntry<TGameContext>> VariableStepHandlers { get; }

        /// <summary>
        ///   Late step handlers are system level function that run both after an fixed-update step
        ///   and after a variable update step. They should be used sparingly to clean up disposed
        ///   resources, but for all other cases try to stick to the main processing steps.
        /// </summary>
        public List<ActionSystemEntry<TGameContext>> LateStepHandlers { get; }

        public ContextForTimeStepProvider<TGameContext> ContextProvider { get; set; }
        public WorldStepDelegate<TGameContext> WorldStepFunction { get; set; }
        public Func<bool> IsWaitingForInputDelegate { get; set; }

        bool IsWaitingForInput()
        {
            return IsWaitingForInputDelegate?.Invoke() ?? true;
        }

        public void Initialize()
        {
            TimeState = new GameTimeState(this.timeProcessor.TimeStepDuration);

            var context = ContextProvider(TimeSpan.Zero, TimeState.FixedGameTimeElapsed);
            foreach (var step in InitializationStepHandlers)
            {
                step.PerformAction(context);
            }
        }


        public void Update(TimeSpan absoluteTime)
        {
            var w = Stopwatch.StartNew();
            logger.Verbose("Enter Update: {Time} ({FixedFrame})", absoluteTime, TimeState.FixedStepCount);
            if (commands.Count > 0)
            {
                // Translates incoming commands into Entity system components. 
                // 
                // Command requests can come in at any time. We deliberately do that processing
                // only at this particular point, where we can guarantee that no one else is
                // modifying the entity system.
                var cmdGameContext = ContextProvider(TimeState.FrameDeltaTime, TimeState.TotalGameTimeElapsed);
                using (LogContext.PushProperty("GameLoop.Activity", "CommandProcessing"))
                {
                    logger.Debug("Processing {CommandCount} commands.", commands.Count);
                    while (commands.TryDequeue(out var command))
                    {
                        command(cmdGameContext);
                    }
                }
            }

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
                                handler.PerformAction(fixedStepContext, "PrepareFixedStep");
                            }
                        }

                        WorldStepFunction?.Invoke(fixedStepContext, TimeState);

                        foreach (var handler in FixedStepHandlers)
                        {
                            using (LogContext.PushProperty("GameLoop.Time", w2.Elapsed.TotalMilliseconds))
                            {
                                handler.PerformAction(fixedStepContext, "FixedStep");
                            }
                        }

                        foreach (var stepHandler in LateStepHandlers)
                        {
                            using (LogContext.PushProperty("GameLoop.Time", w2.Elapsed.TotalMilliseconds))
                            {
                                stepHandler.PerformAction(fixedStepContext, "LateFixedStep");
                            }
                        }

                        logger.Verbose("Processed frame in {Elapsed} ({ElapsedTotalMilliseconds})", w2.Elapsed, w2.Elapsed.TotalMilliseconds);
                    } while (!IsWaitingForInput() && count < budget);

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
                        stepHandler.PerformAction(gameContext, "Variable");
                    }
                }

                foreach (var stepHandler in LateStepHandlers)
                {
                    using (LogContext.PushProperty("GameLoop.Time", w.Elapsed.TotalMilliseconds))
                    {
                        stepHandler.PerformAction(gameContext, "Variable-LateStep");
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