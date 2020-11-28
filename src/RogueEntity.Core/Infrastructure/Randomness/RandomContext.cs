using System;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Randomness.PCGSharp;

namespace RogueEntity.Core.Infrastructure.Randomness
{
    public static class RandomContext
    {
        public static ulong MakeSeed<TEntity>(TEntity entity,
                                            int seedVariance) 
            where TEntity : IRandomSeedSource
        {
            var retval = ((ulong)entity.AsRandomSeed()) << 32;
            return retval + (ulong)seedVariance;
        }

        public static Func<double> DefaultRandomGenerator<TEntity>(TEntity entity,
                                                                   int seedVariance) 
            where TEntity : IRandomSeedSource
        {
            var seed = MakeSeed(entity, seedVariance);
            var r = new PCG(seed);
            return r.NextDouble;
        }

        public static EntityRandomSeedSource<TEntityKey> ToRandomSeedSource<TEntityKey>(this TEntityKey k) 
            where TEntityKey : IEntityKey
        {
            return new EntityRandomSeedSource<TEntityKey>(k);
        }
    }
}