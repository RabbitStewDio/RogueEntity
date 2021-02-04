using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;

namespace RogueEntity.Benchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class ConsoleBenchmark
    {
        char[] consoleChars;
        Random random;

        [Benchmark]
        public void PrintConsoleArray()
        {
            for (int i = 0; i < 80 * 25; i += 1)
            {
                consoleChars[i] = ((char)random.Next('A', 'Z'));
            }
            Console.Out.Write(consoleChars);
        }
        
        [Benchmark]
        public void PrintConsoleDirect()
        {
            for (int i = 0; i < 80 * 25; i += 1)
            {
                Console.Out.Write((char)random.Next('A', 'Z'));
            }
        }

        [GlobalSetup]
        public void SetUpGlobal()
        {
            random = new Random();
            consoleChars = new char[80 * 25];
        }

    }
}
