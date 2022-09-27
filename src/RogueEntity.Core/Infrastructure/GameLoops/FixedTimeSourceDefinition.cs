using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using Serilog;
using System;

namespace RogueEntity.Core.Infrastructure.GameLoops
{
    public class FixedTimeSourceDefinition : ITimeSourceDefinition
    {
        static readonly ILogger logger = SLog.ForContext(typeof(FixedTimeSourceDefinition));
        
        readonly int stepsPerFrame;
        readonly TimeSpan timePerTick;

        public FixedTimeSourceDefinition(int stepsPerFrame, TimeSpan timePerTick)
        {
            this.stepsPerFrame = stepsPerFrame;
            this.timePerTick = timePerTick;
            UpdateTicksPerSecond = 1 / timePerTick.TotalSeconds;
        }

        public double UpdateTicksPerSecond { get; }

        public IGameLoop BuildTimeStepLoop(IGameLoopSystemInformation t)
        {
            var s = new FixedTimeStepGameLoop(this, stepsPerFrame, timePerTick);
            foreach (var entry in t.InitializationEntries)
            {
                logger.Debug("Adding Initialization Step {StepName}", entry.ToString());
                s.InitializationStepHandlers.Add(entry);
            }
            foreach (var entry in t.PreFixedStepEntries)
            {
                logger.Debug("Adding Pre-Fixed Step {StepName}", entry.ToString());
                s.PreFixedStepHandlers.Add(entry);
            }
            foreach (var entry in t.FixedStepEntries)
            {
                logger.Debug("Adding Fixed Step {StepName}", entry.ToString());
                s.FixedStepHandlers.Add(entry);
            }
            foreach (var entry in t.LateFixedStepEntries)
            {
                logger.Debug("Adding Late-Fixed Step {StepName}", entry.ToString());
                s.LateVariableStepHandlers.Add(entry);
            }
            foreach (var entry in t.VariableStepEntries)
            {
                logger.Debug("Adding Variable Step {StepName}", entry.ToString());
                s.VariableStepHandlers.Add(entry);
            }
            foreach (var entry in t.LateVariableStepEntries)
            {
                logger.Debug("Adding Late Variable Step {StepName}", entry.ToString());
                s.LateFixedStepHandlers.Add(entry);
            }
            foreach (var entry in t.DisposeEntries)
            {
                logger.Debug("Adding Dispose Step {StepName}", entry.ToString());
                s.DisposeStepHandlers.Add(entry);
            }

            return s;
        }
    }

    public class RealTimeSourceDefinition : ITimeSourceDefinition
    {
        static readonly ILogger logger = SLog.ForContext(typeof(RealTimeSourceDefinition));

        public RealTimeSourceDefinition(double ticksPerSecond)
        {
            UpdateTicksPerSecond = ticksPerSecond;
        }

        public double UpdateTicksPerSecond { get; }

        public IGameLoop BuildTimeStepLoop(IGameLoopSystemInformation t)
        {
            var s = new RealTimeGameLoop(this, UpdateTicksPerSecond);
            foreach (var entry in t.InitializationEntries)
            {
                logger.Debug("Adding Initialization Step {StepName}", entry.ToString());
                s.InitializationStepHandlers.Add(entry);
            }

            foreach (var entry in t.PreFixedStepEntries)
            {
                logger.Debug("Adding Pre-Fixed Step {StepName}", entry.ToString());
                s.PreFixedStepHandlers.Add(entry);
            }

            foreach (var entry in t.FixedStepEntries)
            {
                logger.Debug("Adding Fixed Step {StepName}", entry.ToString());
                s.FixedStepHandlers.Add(entry);
            }

            foreach (var entry in t.LateFixedStepEntries)
            {
                logger.Debug("Adding Late-Fixed Step {StepName}", entry.ToString());
                s.LateVariableStepHandlers.Add(entry);
            }

            foreach (var entry in t.VariableStepEntries)
            {
                logger.Debug("Adding Variable Step {StepName}", entry.ToString());
                s.VariableStepHandlers.Add(entry);
            }

            foreach (var entry in t.LateVariableStepEntries)
            {
                logger.Debug("Adding Late Variable Step {StepName}", entry.ToString());
                s.LateFixedStepHandlers.Add(entry);
            }

            foreach (var entry in t.DisposeEntries)
            {
                logger.Debug("Adding Dispose Step {StepName}", entry.ToString());
                s.DisposeStepHandlers.Add(entry);
            }

            return s;
        }
    }
}
