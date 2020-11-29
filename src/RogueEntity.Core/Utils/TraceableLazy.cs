using System;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;

namespace RogueEntity.Core.Utils
{
    public interface ITraceableObject
    {
        bool TryGetTraceInfo(out StackTrace s);
    }
    
    public class TraceableLazy<T> : Lazy<T>, ITraceableObject
    {
        void CollectTraceInformation()
        {
#if DEBUG
            TraceInfo = new StackTrace(3);
#endif
        }

#if DEBUG
        StackTrace TraceInfo { get; private set; }
#endif

        public bool TryGetTraceInfo(out StackTrace s)
        {
            
#if DEBUG
            s = TraceInfo;
            return true;
#else
            s = default;
            return false;
#endif
        }

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