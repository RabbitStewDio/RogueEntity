using System;
using RogueEntity.Api.Utils;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Utils.DataViews
{
    public interface IReadOnlyDynamicDataView2D<T> : IReadOnlyView2D<T>
    {
        public event EventHandler<DynamicDataView2DEventArgs<T>>? ViewChunkCreated;
        public event EventHandler<DynamicDataView2DEventArgs<T>>? ViewChunkExpired;

        int OffsetX { get; }
        int OffsetY { get; }
        int TileSizeX { get; }
        int TileSizeY { get; }

        BufferList<Rectangle> GetActiveTiles(BufferList<Rectangle>? data = null);
        Rectangle GetActiveBounds();
        bool TryGetData(int x, int y, [MaybeNullWhen(false)] out IReadOnlyBoundedDataView<T> raw);
    }
}