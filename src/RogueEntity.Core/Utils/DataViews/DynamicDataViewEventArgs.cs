using System;

namespace RogueEntity.Core.Utils.DataViews
{
    public class DynamicDataViewEventArgs<T> : EventArgs
    {
        public readonly BoundedDataView<T> Data;

        public DynamicDataViewEventArgs(BoundedDataView<T> data)
        {
            this.Data = data;
        }
    }
}