namespace RogueEntity.Core.Utils.DataViews
{
    public interface IBoundedDataViewPool<T>
    {
        DynamicDataViewConfiguration TileConfiguration { get; }
        IPooledBoundedDataView<T> Lease(Rectangle bounds, long time);
        void Return(IPooledBoundedDataView<T> leased);
    }
}