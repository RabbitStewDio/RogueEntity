using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Utils.DataViews;

public interface IBoundedDataView<TData>: IView2D<TData>, IReadOnlyBoundedDataView<TData>
{
    [return: NotNullIfNotNull("defaultValue")]
    ref TData? TryGetForUpdate(int x, int y, ref TData? defaultValue, out bool success);
}