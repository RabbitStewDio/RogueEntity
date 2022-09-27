using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace RogueEntity.Core.Utils
{
    public interface ITraceableObject
    {
        bool TryGetTraceInfo([MaybeNullWhen(false)] out StackTrace s);
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
        StackTrace? TraceInfo { get; set; }
#endif

        public bool TryGetTraceInfo([MaybeNullWhen(false)] out StackTrace s)
        {
            
#if DEBUG
            if (TraceInfo == null)
            {
                s = default;
                return false;
            }
            
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

        public TraceableLazy(Func<T> valueFactory) : base(valueFactory)
        {
            CollectTraceInformation();
        }

        public TraceableLazy(Func<T> valueFactory, bool isThreadSafe) : base(valueFactory, isThreadSafe)
        {
            CollectTraceInformation();
        }

        public TraceableLazy(Func<T> valueFactory, LazyThreadSafetyMode mode) : base(valueFactory, mode)
        {
            CollectTraceInformation();
        }

        public TraceableLazy(LazyThreadSafetyMode mode) : base(mode)
        {
            CollectTraceInformation();
        }
    }
}