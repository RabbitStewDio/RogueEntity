using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace RogueEntity.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net472, baseline: true)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class GoalFinderBenchmarkMaze256: GoalFinderBenchmarkBase
    {
        public GoalFinderBenchmarkMaze256() : base("Maze256.txt")
        {
        }
        
        [Benchmark]
        public void GoalMaze256()
        {
            ValidatePathFinding();
        }

        [GlobalSetup]
        public override void SetUpGlobal()
        {
            base.SetUpGlobal();
        }
    }
}
