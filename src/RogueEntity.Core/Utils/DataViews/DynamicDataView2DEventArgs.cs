using System;

namespace RogueEntity.Core.Utils.DataViews
{
    public readonly struct DynamicDataView2DEventArgs<T>
    {
        public readonly TileIndex Key;
        public readonly IReadOnlyBoundedDataView<T> Data;

        public DynamicDataView2DEventArgs(TileIndex key, IReadOnlyBoundedDataView<T> data)
        {
            this.Key = key;
            this.Data = data;
        }
    }
}