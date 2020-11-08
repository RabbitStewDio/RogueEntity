namespace RogueEntity.Core.Utils.DataViews
{
    public interface IDynamicDataView2D<T> : IReadOnlyDynamicDataView2D<T>, IView2D<T>
    {
        bool TryGetWriteAccess(int x, int y, out IBoundedDataView<T> raw, DataViewCreateMode mode = DataViewCreateMode.Nothing);
        bool TryGetRawAccess(int x, int y, out IBoundedDataViewRawAccess<T> raw);
    }
}