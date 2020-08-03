using System;
using EnttSharp.Entities;

namespace RogueEntity.Core.Infrastructure
{
    public interface IRandomContext
    {
        public Func<double> RandomGenerator<TEntity>(TEntity entity, int seedVariance) where TEntity: IEntityKey;
    }
}