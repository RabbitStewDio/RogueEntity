using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Utils.DataViews
{
    public interface IDynamicDataView2D<T> : IReadOnlyDynamicDataView2D<T>, IView2D<T>
    {
        bool TryGetWriteAccess(int x, int y, [MaybeNullWhen(false)] out IBoundedDataView<T> raw, DataViewCreateMode mode = DataViewCreateMode.Nothing);
        bool TryGetRawAccess(int x, int y, [MaybeNullWhen(false)] out IBoundedDataViewRawAccess<T> raw);

        ref T? TryGetForUpdate(int x, int y, ref T? defaultValue, out bool success, DataViewCreateMode mode = DataViewCreateMode.Nothing);
    }
}