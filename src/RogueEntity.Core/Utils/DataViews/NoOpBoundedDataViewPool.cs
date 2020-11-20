namespace RogueEntity.Core.Utils.DataViews
{
    public class NoOpBoundedDataViewPool<T>: IBoundedDataViewPool<T>
    {
        public NoOpBoundedDataViewPool(DynamicDataViewConfiguration tileConfiguration)
        {
            TileConfiguration = tileConfiguration;
        }

        public DynamicDataViewConfiguration TileConfiguration { get; }
        
        public IPooledBoundedDataView<T> Lease(Rectangle bounds, long time)
        {
            return new DefaultPooledBoundedDataView<T>(bounds, time);
        }

        public void Return(IPooledBoundedDataView<T> leased)
        {
        }
    }
}