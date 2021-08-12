using System;

namespace RogueEntity.Core.Utils.DataViews
{
    public class DynamicDataView2DEventArgs<T> : EventArgs
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