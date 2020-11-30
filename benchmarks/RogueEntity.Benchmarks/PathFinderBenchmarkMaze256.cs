using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace RogueEntity.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net472, baseline: true)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class PathFinderBenchmarkMaze256: PathFinderBenchmarkBase
    {
        public PathFinderBenchmarkMaze256() : base("Maze256.txt")
        {
        }
        
        [Benchmark]
        public void BenchmarkMaze256()
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