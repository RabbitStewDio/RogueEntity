namespace RogueEntity.Core.Infrastructure.Randomness
{
    public interface IEntityRandomGeneratorSource
    {
        /// <summary>
        ///   Produces a function that represents a deterministic pseud-random number generator
        ///   seeded with the given entity and seed variance.
        ///
        ///   The function produces a value between 0 (inclusive) and 1 (exclusive). 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="seedVariance"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public IRandomGenerator RandomGenerator<TEntity>(TEntity entity, int seedVariance) where TEntity: IRandomSeedSource;
    }
}