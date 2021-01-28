using System;

namespace RogueEntity.Core.Utils.DataViews
{
    public class DynamicDataView2DEventArgs<T> : EventArgs
    {
        public readonly Position2D Key;
        public readonly IReadOnlyBoundedDataView<T> Data;

        public DynamicDataView2DEventArgs(Position2D key, IReadOnlyBoundedDataView<T> data)
        {
            this.Key = key;
            this.Data = data;
        }
    }
}