using System;

namespace RogueEntity.Core.Utils.DataViews
{
    public readonly struct DynamicDataView3DEventArgs<T>
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