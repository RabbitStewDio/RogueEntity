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
    ///   This game loop keeps ticking even if the user does not perform any
    ///   actions. This behaviour is similar to Unity and other game engines.
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    public class RealTimeGameLoop<TGameContext> : ITimeSource, IGameLoop<TGameContext>, ISystemGameLoop<TGameContext>
    {
        /// <summary>
        ///   An optional time span defining how much simulated time passes on each fixed step.
        /// </summary>
        readonly Optional<TimeSpan> gameFixedTimeStep;

        readonly ILogger logger = SLog.ForContext<RealTimeGameLoop<TGameContext>>();

        readonly GameTimeProcessor timeProcessor;
        readonly Queue<Action<TGameContext>> commands;

        public RealTimeGameLoop(int fps = 60, Optional<TimeSpan> gameFixedTimeStep = default)
        {
            this.gameFixedTimeStep = gameFixedTimeStep;
            timeProcessor = GameTimeProcessor.WithFramesPerSecond(fps);

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

        /// <summary>
        ///   A global set of handlers that runs once at the start of  each new game.
        ///   Use them to initialize the world after loading or generating content.
        /// </summary>
        public List<ActionSystemEntry<TGameContext>> InitializationStepHandlers { get; }

        public List<ActionSystemEntry<TGameContext>> FixedStepHandlers { get; }

        public List<ActionSystemEntry<TGameContext>> PreFixedStepHandlers { get; }

        public List<ActionSystemEntry<TGameContext>> VariableStepHandlers { get; }

        /// <summary>
        ///   Those handlers are system handlers and are called after each fixed or variable
        ///   time step.
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
                step.PerformAction(context);
            }
        }

        TimeSpan fixedTimeUpdateTargetTime;
        TimeSpan fixedTimeUpdateHandledTime;

        public void Update(TimeSpan absoluteTime)
        {
            var w = Stopwatch.StartNew();
            logger.Verbose("Enter Update: {Time}", absoluteTime);
            if (commands.Count > 0)
            {
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
                        stepHandler.PerformAction(gameContext, "VariableStep");
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