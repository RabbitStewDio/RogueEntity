using System.Collections.Generic;

namespace RogueEntity.Core.Utils.DataViews
{
    public interface IReadOnlyDynamicDataView2D<T> : IReadOnlyView2D<T>
    {
        int OffsetX { get; }
        int OffsetY { get; }
        int TileSizeX { get; }
        int TileSizeY { get; }

        List<Rectangle> GetActiveTiles(List<Rectangle> data = null);
        Rectangle GetActiveBounds();
        bool TryGetData(int x, int y, out IReadOnlyBoundedDataView<T> raw);
    }
}