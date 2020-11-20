using System.Threading;

namespace RogueEntity.Core.Utils.DataViews
{
    public class DefaultPooledBoundedDataView<T> : BoundedDataView<T>, IPooledBoundedDataView<T>
    {
        long currentTime;
        int usedForReading;
        int usedForWriting;

        public DefaultPooledBoundedDataView(in Rectangle bounds, long time) : base(in bounds)
        {
            currentTime = time;
            LastUsed = time;
            usedForReading = 0;
            usedForWriting = 0;
        }

        public DefaultPooledBoundedDataView(Rectangle bounds, T[] data) : base(bounds, data)
        {
        }

        public long LastUsed { get; private set; }

        public void BeginUseTimePeriod(long time)
        {
            lock (this)
            {
                currentTime = time;
                usedForReading = 0;
                usedForWriting = 0;
            }
        }

        public void MarkUsedForReading()
        {
            Interlocked.CompareExchange(ref usedForReading, 1, 0);
        }

        public void MarkUsedForWriting()
        {
            Interlocked.CompareExchange(ref usedForReading, 1, 0);
            Interlocked.CompareExchange(ref usedForWriting, 1, 0);
        }

        public bool IsUsedForReading => usedForReading != 0;
        public bool IsUsedForWriting => usedForWriting != 0;

        public void CommitUseTimePeriod()
        {
            lock (this)
            {
                if (usedForReading == 1)
                {
                    LastUsed = currentTime;
                }
            }
        }

        public static DefaultPooledBoundedDataView<T> CreateForPool()
        {
            return new DefaultPooledBoundedDataView<T>(new Rectangle(), 0);
        }
    }
}