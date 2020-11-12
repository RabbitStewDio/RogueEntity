using System;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.GameLoops
{
    public static class GameLoopExtensions
    {
        static readonly ILogger Logger = SLog.ForContext(typeof(GameLoopExtensions)); 
        
        public static IGameLoop<TGameContext> BuildFixedTimeStepLoop<TGameContext>(this IGameLoopSystemInformation<TGameContext> t, int maxSteps, TimeSpan deltaTime)
        {
            var s = new FixedTimeStepGameLoop<TGameContext>(maxSteps, deltaTime);
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
        
        public static IGameLoop<TGameContext> BuildRealTimeStepLoop<TGameContext>(this IGameLoopSystemInformation<TGameContext> t, int fps, Optional<TimeSpan> fixedStepTime = default)
        {
            var s = new RealTimeGameLoop<TGameContext>(fps, fixedStepTime);
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