namespace RogueEntity.Core.Utils.Maps
{
    public interface IBoundedDataViewRawAccess<TData>: IView2D<TData>
    {
        Rectangle Bounds { get; }
        TData[] Data { get; }
    }
}