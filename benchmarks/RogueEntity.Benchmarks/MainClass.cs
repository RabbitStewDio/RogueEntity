using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using JetBrains.Annotations;

#pragma warning disable 162

namespace RogueEntity.Benchmarks
{
    [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
    public class MainClass
    {
        const bool RunManually = false;

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public static void Main(string[] args)
        {
            if (RunManually)
            { 
                RunOnce_GoalFinder();
                return;
            }

            var config = new ManualConfig();
            config.Add(DefaultConfig.Instance);
            config.Add(MemoryDiagnoser.Default);
            // config.Add(new InliningDiagnoser(true, new[] {"EnTTSharp", "RogueEntity"}));

            BenchmarkRunner.Run(typeof(MainClass).Assembly, config);
        }

        static void RunOnce_GoalFinder()
        {
            var bm = new GoalFinderBenchmarkMaze256();
            bm.SetUpGlobal();
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 200; i += 1)
            {
                if ((i % 50) == 0)
                {
                    //MemoryProfiler.GetSnapshot();
                }

                sw.Restart();
                bm.GoalMaze256();
                // Console.WriteLine(i + " " + sw.Elapsed);
            }
        }

        [UsedImplicitly]
        static void RunOnce_PathFinder()
        {
            var bm = new PathFinderBenchmarkMaze256();
            bm.SetUpGlobal();
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 200; i += 1)
            {
                sw.Restart();
                bm.PathFinderMaze256();
                // Console.WriteLine(i + " " + sw.Elapsed);
            }
        }
    }
}
