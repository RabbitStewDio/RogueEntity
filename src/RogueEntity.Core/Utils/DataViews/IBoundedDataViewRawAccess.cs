namespace RogueEntity.Core.Utils.DataViews
{
    public interface IBoundedDataViewRawAccess<TData>: IBoundedDataView<TData>
    {
        TData[] Data { get; }
    }
}