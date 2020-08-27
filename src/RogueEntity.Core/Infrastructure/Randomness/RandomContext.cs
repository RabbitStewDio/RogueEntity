using System;
using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Randomness
{
    public static class RandomContext
    {
        public static int MakeSeed<TEntity>(TEntity entity,
                                            int seedVariance) 
            where TEntity : IRandomSeedSource
        {
            unchecked
            {
                return entity.AsRandomSeed() * 319 + seedVariance;
            }
        }

        public static Func<double> DefaultRandomGenerator<TEntity>(TEntity entity,
                                                                   int seedVariance) 
            where TEntity : IRandomSeedSource
        {
            var seed = MakeSeed(entity, seedVariance);
            var r = new Random(seed);
            return r.NextDouble;
        }

        public static EntityRandomSeedSource<TEntityKey> ToRandomSeedSource<TEntityKey>(this TEntityKey k) 
            where TEntityKey : IEntityKey
        {
            return new EntityRandomSeedSource<TEntityKey>(k);
        }
    }
}