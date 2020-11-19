using System;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;

namespace RogueEntity.Core.Utils
{
    public interface ITraceableObject
    {
        StackTrace TraceInfo { get; }
    }
    
    public class TraceableLazy<T> : Lazy<T>, ITraceableObject
    {
#if DEBUG
#endif
        void CollectTraceInformation()
        {
#if DEBUG
            TraceInfo = new StackTrace(3);
#endif
        }

        public StackTrace TraceInfo { get; private set; }

        public TraceableLazy()
        {
            CollectTraceInformation();
        }

        public TraceableLazy(bool isThreadSafe) : base(isThreadSafe)
        {
            CollectTraceInformation();
        }

        public TraceableLazy([NotNull] Func<T> valueFactory) : base(valueFactory)
        {
            CollectTraceInformation();
        }

        public TraceableLazy([NotNull] Func<T> valueFactory, bool isThreadSafe) : base(valueFactory, isThreadSafe)
        {
            CollectTraceInformation();
        }

        public TraceableLazy([NotNull] Func<T> valueFactory, LazyThreadSafetyMode mode) : base(valueFactory, mode)
        {
            CollectTraceInformation();
        }

        public TraceableLazy(LazyThreadSafetyMode mode) : base(mode)
        {
            CollectTraceInformation();
        }
    }
}