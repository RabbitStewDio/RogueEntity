using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Utils.DataViews
{
    public interface IReadOnlyBoundedDataView<TData> : IReadOnlyView2D<TData>
    {
        Rectangle Bounds { get; }
        bool Contains(int x, int y);
    }
    
    public interface IBoundedDataView<TData>: IView2D<TData>, IReadOnlyBoundedDataView<TData>
    {
        [return: NotNullIfNotNull("defaultValue")]
        ref TData? TryGetForUpdate(int x, int y, ref TData? defaultValue, out bool success);
    }
    
    public interface IBoundedDataViewRawAccess<TData>: IBoundedDataView<TData>
    {
        TData[] Data { get; }
    }
}