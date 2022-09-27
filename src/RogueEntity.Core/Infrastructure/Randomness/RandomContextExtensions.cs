using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Randomness
{
    public static class RandomContextExtensions
    {
        public static IRandomGenerator FromEntity<TEntity>(this IEntityRandomGeneratorSource generatorSource,
                                                           TEntity entity,
                                                           int seedVariance)
            where TEntity : struct, IEntityKey
        {
            return generatorSource.RandomGenerator(new EntityRandomSeedSource<TEntity>(entity), seedVariance);
        }

        public static IRandomGenerator FromConstantSeed(this IEntityRandomGeneratorSource generatorSource,
                                                        int seedVariance,
                                                        int seed = 0)
        {
            return generatorSource.RandomGenerator(new ConstantRandomSeedSource(seed), seedVariance);
        }
        
        public static int Next(this IRandomGenerator g, int lowerBound, int upperBoundExclusive)
        {
            if (lowerBound >= upperBoundExclusive)
            {
                return lowerBound;
            }

            var delta = upperBoundExclusive - lowerBound;
            var floor = (int)(g.Next() * delta);
            if (floor == delta)
            {
                // cheap hack. When using float the system happily 
                // rounds up.
                floor = delta - 1;
            }
            return floor + lowerBound;
        }

    }
}