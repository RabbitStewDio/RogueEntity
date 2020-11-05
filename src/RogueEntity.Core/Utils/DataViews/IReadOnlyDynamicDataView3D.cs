using System.Collections.Generic;

namespace RogueEntity.Core.Utils.DataViews
{
    public interface IReadOnlyDynamicDataView3D<T>
    {
        int OffsetX { get; }
        int OffsetY { get; }
        int TileSizeX { get; }
        int TileSizeY { get; }

        bool TryGetView(int z, out IReadOnlyDynamicDataView2D<T> view);
        List<int> GetActiveLayers(List<int> buffer = null);
    }
}