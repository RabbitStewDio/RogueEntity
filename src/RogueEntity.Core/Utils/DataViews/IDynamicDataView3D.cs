using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Utils.DataViews
{
    public interface IDynamicDataView3D<T>: IReadOnlyDynamicDataView3D<T>
    {
        bool TryGetWritableView(int z, [MaybeNullWhen(false)] out IDynamicDataView2D<T> view, DataViewCreateMode mode = DataViewCreateMode.Nothing);
        bool RemoveView(int z);
        void Clear();
    }
}