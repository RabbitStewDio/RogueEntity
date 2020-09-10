using System;
using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Randomness
{
    public static class RandomContextExtensions
    {
        public static Func<double> RandomGeneratorFromEntity<TEntity>(this IRandomContext context, 
                                                                      TEntity entity, 
                                                                      int seedVariance) where TEntity : IEntityKey
        {
            return context.RandomGenerator(new EntityRandomSeedSource<TEntity>(entity), seedVariance);
        }
    }
}