using System;

namespace RogueEntity.Core.Infrastructure.Randomness
{
    public interface IRandomContext
    {
        public Func<double> RandomGenerator<TEntity>(TEntity entity, int seedVariance) where TEntity: IRandomSeedSource;
    }
}