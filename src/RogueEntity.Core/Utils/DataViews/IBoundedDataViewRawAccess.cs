namespace RogueEntity.Core.Utils.DataViews
{
    public interface IReadOnlyBoundedDataView<TData> : IReadOnlyView2D<TData>
    {
        Rectangle Bounds { get; }
    }
    
    public interface IBoundedDataView<TData>: IView2D<TData>, IReadOnlyBoundedDataView<TData>
    {
    }
    
    public interface IBoundedDataViewRawAccess<TData>: IBoundedDataView<TData>
    {
        TData[] Data { get; }
    }
}