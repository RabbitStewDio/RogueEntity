using System.Collections.Generic;
using GoRogue;

namespace RogueEntity.Core.Utils.Maps
{
    public interface IDynamicDataView2D<T> : IView2D<T>
    {
        int OffsetX { get; }
        int OffsetY { get; }
        int TileSizeX { get; }
        int TileSizeY { get; }

        List<Rectangle> GetActiveTiles(List<Rectangle> data = null);
        bool TryGetData(int x, int y, out IBoundedDataViewRawAccess<T> raw);
    }
}