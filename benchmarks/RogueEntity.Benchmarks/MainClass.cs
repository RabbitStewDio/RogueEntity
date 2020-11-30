using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Running;

namespace RogueEntity.Benchmarks
{
    public class MainClass
    {
        const bool RunManually = false;

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public static void Main(string[] args)
        {
            if (RunManually)
            {
                RunOnce();
                return;
            }

            var summary = BenchmarkRunner.Run(typeof(MainClass).Assembly);
            Console.WriteLine(summary);
        }

        static void RunOnce()
        {
            var bm = new PathFinderBenchmarkMaze256();
            bm.SetUpGlobal();
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 20; i += 1)
            {
                if (i == 4)
                {
                    Console.WriteLine("HERE");
                }

                sw.Restart();
                bm.BenchmarkMaze256();
                Console.WriteLine(i + " " + sw.Elapsed);
            }
        }
    }
}