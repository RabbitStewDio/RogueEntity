using JetBrains.Annotations;
using Microsoft.Extensions.ObjectPool;

namespace RogueEntity.Core.Utils.DataViews
{
    public class DefaultBoundedDataViewPool<T> : IBoundedDataViewPool<T>
    {
        readonly ObjectPool<DefaultPooledBoundedDataView<T>> pool;

        public DefaultBoundedDataViewPool([NotNull] ObjectPoolProvider poolProvider,
                                          DynamicDataViewConfiguration config)
        {
            this.pool = poolProvider.Create(new Policy());
            this.TileConfiguration = config;
        }

        public DynamicDataViewConfiguration TileConfiguration { get; }

        public IPooledBoundedDataView<T> Lease(Rectangle bounds, long time)
        {
            var result = this.pool.Get();
            result.Resize(bounds, true);
            result.BeginUseTimePeriod(time);
            return result;
        }

        public void Return(IPooledBoundedDataView<T> leased)
        {
            if (leased is DefaultPooledBoundedDataView<T> leasedDefault)
            {
                pool.Return(leasedDefault);
            }
        }

        class Policy : IPooledObjectPolicy<DefaultPooledBoundedDataView<T>>
        {
            public DefaultPooledBoundedDataView<T> Create()
            {
                return DefaultPooledBoundedDataView<T>.CreateForPool();
            }

            public bool Return(DefaultPooledBoundedDataView<T> obj)
            {
                return true;
            }
        }
    }
}