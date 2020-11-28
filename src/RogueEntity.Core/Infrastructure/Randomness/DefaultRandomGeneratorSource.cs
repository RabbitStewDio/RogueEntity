using System;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Infrastructure.Randomness.PCGSharp;

namespace RogueEntity.Core.Infrastructure.Randomness
{
    public class DefaultRandomGeneratorSource: IEntityRandomGeneratorSource
    {
        readonly DefaultObjectPool<Generator> pool;

        public DefaultRandomGeneratorSource()
        {
            pool = new DefaultObjectPool<Generator>(new GeneratorPolicy(Return));
        }

        void Return(Generator obj)
        {
            pool.Return(obj);
        }
        
        public IRandomGenerator RandomGenerator<TSeedSource>(TSeedSource entity, int seedVariance)
            where TSeedSource : IRandomSeedSource
        {
            var generator = pool.Get();
            generator.Activate(RandomContext.MakeSeed(entity, seedVariance));
            return generator;
        }

        class Generator: IRandomGenerator
        {
            readonly PCG randomSource;
            readonly Action<Generator> pool;
            bool disposed;

            public Generator(Action<Generator> returnCallback)
            {
                this.randomSource = new PCG(0);
                this.pool = returnCallback;
            }

            public void Activate(ulong seed)
            {
                disposed = false;
                randomSource.Initialize(seed);
            }
            
            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    pool?.Invoke(this);
                }
            }

            public double Next()
            {
                return randomSource.NextDouble();
            }
        }
        
        class GeneratorPolicy: IPooledObjectPolicy<Generator>
        {
            readonly Action<Generator> returnCallback;

            public GeneratorPolicy(Action<Generator> returnCallback)
            {
                this.returnCallback = returnCallback;
            }

            public Generator Create()
            {
                return new Generator(returnCallback);
            }

            public bool Return(Generator obj)
            {
                return true;
            }
        }
    }
}