using System;

namespace RogueEntity.Core.Utils.DataViews
{
    public class DynamicDataView3DEventArgs<T> : EventArgs
    {
        public readonly int ZLevel;
        public readonly IReadOnlyDynamicDataView2D<T> Data;

        public DynamicDataView3DEventArgs(int zLevel, IReadOnlyDynamicDataView2D<T> data)
        {
            this.ZLevel = zLevel;
            this.Data = data;
        }
    }
}