using System;

namespace RogueEntity.Core.Utils.Maps
{
    public class DynamicBoolDataViewEventArgs : EventArgs
    {
        public readonly BoundedBoolDataView Data;

        public DynamicBoolDataViewEventArgs(BoundedBoolDataView data)
        {
            this.Data = data;
        }
    }
}