using RogueEntity.Core.Infrastructure.Randomness.PCGSharp;

namespace RogueEntity.Core.Infrastructure.Randomness
{
    public class FixedRandomGeneratorSource : IEntityRandomGeneratorSource
    {
        readonly ulong seed;

        public FixedRandomGeneratorSource(ulong seed = 0)
        {
            this.seed = seed;
        }

        public IRandomGenerator RandomGenerator(int seedVariance)
        {
            return new FixedGenerator(RandomContext.MakeSeed(seed, seedVariance));
        }

        public IRandomGenerator RandomGenerator<TEntity>(TEntity entity, int seedVariance)
            where TEntity : IRandomSeedSource
        {
            return new FixedGenerator(RandomContext.MakeSeed(seed, entity, seedVariance));
        }

        class FixedGenerator : IRandomGenerator
        {
            readonly PCG randomSource;

            public FixedGenerator(ulong seed)
            {
                randomSource = new PCG(seed);
            }

            public void Dispose()
            { }

            public double Next()
            {
                return randomSource.NextDouble();
            }
        }
    }
}
