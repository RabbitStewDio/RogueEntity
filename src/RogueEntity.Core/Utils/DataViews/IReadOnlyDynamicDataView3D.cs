using System;
using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Utils.DataViews
{
    public interface IReadOnlyDynamicDataView3D<T>
    {
        public event EventHandler<DynamicDataView3DEventArgs<T>> ViewCreated;
        public event EventHandler<DynamicDataView3DEventArgs<T>> ViewReset;
        public event EventHandler<DynamicDataView3DEventArgs<T>> ViewExpired;

        int OffsetX { get; }
        int OffsetY { get; }
        int TileSizeX { get; }
        int TileSizeY { get; }

        bool TryGetView(int z, out IReadOnlyDynamicDataView2D<T> view);
        BufferList<int> GetActiveLayers(BufferList<int> buffer = null);
    }
}