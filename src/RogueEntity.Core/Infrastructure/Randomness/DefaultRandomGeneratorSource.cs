using System;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Time;
using RogueEntity.Core.Infrastructure.Randomness.PCGSharp;

namespace RogueEntity.Core.Infrastructure.Randomness
{
    public class DefaultRandomGeneratorSource : IEntityRandomGeneratorSource
    {
        readonly Lazy<ITimeSource> timer;
        readonly DefaultObjectPool<Generator> pool;
        int seed;

        public DefaultRandomGeneratorSource(int seed, Lazy<ITimeSource> timer)
        {
            this.seed = seed;
            this.timer = timer;
            this.pool = new DefaultObjectPool<Generator>(new GeneratorPolicy(Return));
        }

        public int Seed
        {
            get => seed;
            set => seed = value;
        }

        void Return(Generator obj)
        {
            pool.Return(obj);
        }

        public IRandomGenerator RandomGenerator(int seedVariance)
        {
            var generator = pool.Get();
            ulong timeBasedSeed = RandomContext.Combine((ulong)this.seed << 32, (ulong)(timer.Value.FixedStepFrameCounter));
            generator.Activate(RandomContext.MakeSeed(timeBasedSeed, seedVariance));
            return generator;
        }

        public IRandomGenerator RandomGenerator<TSeedSource>(TSeedSource entity, int seedVariance)
            where TSeedSource : IRandomSeedSource
        {
            var generator = pool.Get();
            ulong timeBasedSeed = RandomContext.Combine((ulong)this.seed << 32, (ulong)(timer.Value.FixedStepFrameCounter));
            generator.Activate(RandomContext.MakeSeed(timeBasedSeed, entity, seedVariance));
            return generator;
        }

        class Generator : IRandomGenerator
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

        class GeneratorPolicy : IPooledObjectPolicy<Generator>
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
