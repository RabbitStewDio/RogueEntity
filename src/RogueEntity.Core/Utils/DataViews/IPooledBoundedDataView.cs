using System;

namespace RogueEntity.Core.Utils.DataViews
{
    public interface IPooledBoundedDataView<TData>: IBoundedDataViewRawAccess<TData>, IEquatable<BoundedDataView<TData>>
    {
        long LastUsed { get; }
        void BeginUseTimePeriod(long time);
        void MarkUsedForReading();
        void MarkUsedForWriting();
        void CommitUseTimePeriod();
        
        bool IsUsedForReading { get; }
        bool IsUsedForWriting { get; }
    }
}