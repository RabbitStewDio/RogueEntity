using Microsoft.Extensions.ObjectPool;

namespace RogueEntity.Core.Utils.DataViews
{
    public class DefaultBoundedDataViewPool<T> : IBoundedDataViewPool<T>
    {
        readonly ObjectPool<DefaultPooledBoundedDataView<T>> pool;
        
        public DefaultBoundedDataViewPool(DynamicDataViewConfiguration config,
                                          ObjectPoolProvider poolProvider = null)
        {
            if (poolProvider == null)
            {
                var provider = new DefaultObjectPoolProvider();
                provider.MaximumRetained = 512;
                poolProvider = provider;
            }
            
            this.pool = poolProvider.Create(new Policy(config));
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

        class Policy : PooledObjectPolicy<DefaultPooledBoundedDataView<T>>
        {
            readonly DynamicDataViewConfiguration config;

            public Policy(DynamicDataViewConfiguration config)
            {
                this.config = config;
            }

            public override DefaultPooledBoundedDataView<T> Create()
            {
                return DefaultPooledBoundedDataView<T>.CreateForPool(config);
            }

            public override bool Return(DefaultPooledBoundedDataView<T> obj)
            {
                obj.Clear();
                return true;
            }
        }
    }
}