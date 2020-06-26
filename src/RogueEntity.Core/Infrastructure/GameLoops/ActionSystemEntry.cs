using System;
using System.Diagnostics;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.GameLoops
{
    public class ActionSystemEntry<TGameContext>
    {
        static readonly ILogger Logger = SLog.ForContext<ActionSystemEntry<TGameContext>>();

        readonly Action<TGameContext> action;
        readonly Stopwatch performanceCounter;
        readonly string name;
        readonly int priority;

        public ActionSystemEntry(Action<TGameContext> action, string name, int priority)
        {
            this.action = action;
            this.name = name;
            this.priority = priority;
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

        public string Name
        {
            get { return name; }
        }

        public int Priority
        {
            get { return priority; }
        }

        public double TotalTimeElapsed => performanceCounter.Elapsed.TotalMilliseconds;

        public override string ToString()
        {
            return $"[{priority} - {name}]";
        }
    }
}