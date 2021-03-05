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
        static readonly ILogger Logger = SLog.ForContext(typeof(FixedTimeSourceDefinition));
        
        readonly int stepsPerFrame;
        readonly TimeSpan timePerTick;

        public FixedTimeSourceDefinition(int stepsPerFrame, TimeSpan timePerTick)
        {
            this.stepsPerFrame = stepsPerFrame;
            this.timePerTick = timePerTick;
            TicksPerSecond = 1 / timePerTick.TotalSeconds;
        }

        public double TicksPerSecond { get; }

        public IGameLoop BuildTimeStepLoop(IGameLoopSystemInformation t, Optional<TimeSpan> fixedStepTime = default)
        {
            var s = new FixedTimeStepGameLoop(stepsPerFrame, timePerTick);
            foreach (var entry in t.InitializationEntries)
            {
                Logger.Debug("Adding Initialization Step {StepName}", entry.ToString());
                s.InitializationStepHandlers.Add(entry);
            }
            foreach (var entry in t.PreFixedStepEntries)
            {
                Logger.Debug("Adding Pre-Fixed Step {StepName}", entry.ToString());
                s.PreFixedStepHandlers.Add(entry);
            }
            foreach (var entry in t.FixedStepEntries)
            {
                Logger.Debug("Adding Fixed Step {StepName}", entry.ToString());
                s.FixedStepHandlers.Add(entry);
            }
            foreach (var entry in t.LateFixedStepEntries)
            {
                Logger.Debug("Adding Late-Fixed Step {StepName}", entry.ToString());
                s.LateVariableStepHandlers.Add(entry);
            }
            foreach (var entry in t.VariableStepEntries)
            {
                Logger.Debug("Adding Variable Step {StepName}", entry.ToString());
                s.VariableStepHandlers.Add(entry);
            }
            foreach (var entry in t.LateVariableStepEntries)
            {
                Logger.Debug("Adding Late Variable Step {StepName}", entry.ToString());
                s.LateFixedStepHandlers.Add(entry);
            }
            foreach (var entry in t.DisposeEntries)
            {
                Logger.Debug("Adding Dispose Step {StepName}", entry.ToString());
                s.DisposeStepHandlers.Add(entry);
            }

            return s;
        }
    }

    public class RealTimeSourceDefinition : ITimeSourceDefinition
    {
        static readonly ILogger Logger = SLog.ForContext(typeof(RealTimeSourceDefinition));

        public RealTimeSourceDefinition(double ticksPerSecond)
        {
            TicksPerSecond = ticksPerSecond;
        }

        public double TicksPerSecond { get; }

        public IGameLoop BuildTimeStepLoop(IGameLoopSystemInformation t, Optional<TimeSpan> fixedStepTime = default)
        {
            var s = new RealTimeGameLoop(TicksPerSecond, fixedStepTime);
            foreach (var entry in t.InitializationEntries)
            {
                Logger.Debug("Adding Initialization Step {StepName}", entry.ToString());
                s.InitializationStepHandlers.Add(entry);
            }

            foreach (var entry in t.PreFixedStepEntries)
            {
                Logger.Debug("Adding Pre-Fixed Step {StepName}", entry.ToString());
                s.PreFixedStepHandlers.Add(entry);
            }

            foreach (var entry in t.FixedStepEntries)
            {
                Logger.Debug("Adding Fixed Step {StepName}", entry.ToString());
                s.FixedStepHandlers.Add(entry);
            }

            foreach (var entry in t.LateFixedStepEntries)
            {
                Logger.Debug("Adding Late-Fixed Step {StepName}", entry.ToString());
                s.LateVariableStepHandlers.Add(entry);
            }

            foreach (var entry in t.VariableStepEntries)
            {
                Logger.Debug("Adding Variable Step {StepName}", entry.ToString());
                s.VariableStepHandlers.Add(entry);
            }

            foreach (var entry in t.LateVariableStepEntries)
            {
                Logger.Debug("Adding Late Variable Step {StepName}", entry.ToString());
                s.LateFixedStepHandlers.Add(entry);
            }

            foreach (var entry in t.DisposeEntries)
            {
                Logger.Debug("Adding Dispose Step {StepName}", entry.ToString());
                s.DisposeStepHandlers.Add(entry);
            }

            return s;
        }
    }
}
