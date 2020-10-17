using System;

namespace RogueEntity.Core.Utils.Maps
{
    public class DynamicBoolDataViewEventArgs : EventArgs
    {
        readonly BoundedBoolDataView data;

        public DynamicBoolDataViewEventArgs(BoundedBoolDataView data)
        {
            this.data = data;
        }
    }
}