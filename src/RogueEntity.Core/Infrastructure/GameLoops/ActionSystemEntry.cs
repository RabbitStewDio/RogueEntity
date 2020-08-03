using System;
using System.Diagnostics;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.GameLoops
{
    /// <summary>
    ///   This class represents an component system registration. It also contains supporting
    ///   functionality to measure execution times.
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    public class ActionSystemEntry<TGameContext>
    {
        static readonly ILogger Logger = SLog.ForContext<ActionSystemEntry<TGameContext>>();

        readonly Action<TGameContext> action;
        readonly Stopwatch performanceCounter;

        public ActionSystemEntry(Action<TGameContext> action, string name, int priority)
        {
            this.action = action;
            this.Name = name;
            this.Priority = priority;
            this.performanceCounter = new Stopwatch();
        }

        public void PerformAction(TGameContext context, string contextName = null)
        {
            var w3 = Stopwatch.StartNew();
            performanceCounter.Start();
            try
            {
                action(context);
                if (contextName != null)
                {
                    Logger.Verbose("Pre-Fixed: Processed {Handler} ({HandleTime} / {TotalTime})",
                                   ToString(), w3.Elapsed.TotalMilliseconds, performanceCounter.Elapsed.TotalMilliseconds);
                }
            }
            finally
            {
                performanceCounter.Stop();
            }
        }

        public string Name { get; }

        public int Priority { get; }

        public double TotalTimeElapsed => performanceCounter.Elapsed.TotalMilliseconds;

        public override string ToString()
        {
            return $"[{Priority} - {Name}] - {TotalTimeElapsed}";
        }
    }
}