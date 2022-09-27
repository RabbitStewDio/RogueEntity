using System;
using System.Diagnostics;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Utils;
using Serilog;

namespace RogueEntity.Api.GameLoops
{
    /// <summary>
    ///   This class represents an component system registration. It also contains supporting
    ///   functionality to measure execution times.
    /// </summary>
    public class ActionSystemEntry
    {
        readonly struct ActionSystemTime
        {
            public readonly int InvocationCount;
            public readonly double FrameTime;
            public readonly double TotalTime;

            public ActionSystemTime(int invocationCount, double frameTime, double totalTime)
            {
                InvocationCount = invocationCount;
                FrameTime = frameTime;
                TotalTime = totalTime;
            }

            public override string ToString()
            {
                return $"Invoked {InvocationCount} times; ({FrameTime:0.000} of {TotalTime:0.000} ms";
            }
        }
        
        static readonly ILogger logger = SLog.ForContext<ActionSystemEntry>();

        readonly string text;
        readonly Action action;
        readonly Stopwatch frameCounter;
        readonly Stopwatch performanceCounter;
        int invocationCount;

        public ActionSystemEntry(Action action, ISystemDeclaration? d, int declarationOrder, string context)
        {
            this.action = action;
            this.Name = d?.Id ?? "";
            this.Priority = d?.Priority ?? 0;
            this.DeclarationOrder = declarationOrder;
            this.performanceCounter = new Stopwatch();
            this.frameCounter = new Stopwatch();
            var moduleSuffix = $"@{d?.DeclaringModule}";
            if (string.IsNullOrEmpty(context))
            {
                this.text = $"[{Priority,13:N0} ({DeclarationOrder,4}) - {Name}{moduleSuffix}]";
            }
            else
            {
                this.text = $"[{Priority,13:N0} ({DeclarationOrder,4}) - {Name}{moduleSuffix}<{context}>]";
            }
        }

        public void PerformAction(ActionSystemExecutionContext contextName)
        {
            invocationCount += 1;
            frameCounter.Restart();
            performanceCounter.Start();
            try
            {
                action();
                logger.Verbose("[{Context}] Processed {Handler} ({HandleTime})",
                               contextName, ToString(), 
                               new ActionSystemTime(invocationCount, frameCounter.Elapsed.TotalMilliseconds, performanceCounter.Elapsed.TotalMilliseconds));
            }
            finally
            {
                performanceCounter.Stop();
            }
        }

        public EntitySystemId Name { get; }

        public int Priority { get; }
        public int DeclarationOrder { get; }

        public double TotalTimeElapsed => performanceCounter.Elapsed.TotalMilliseconds;

        public override string ToString()
        {
            return text;
        }
    }
}