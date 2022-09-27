using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Storage
{
    public interface IDataRepository<TKey, TData>
    {
        public IEnumerable<TKey> QueryEntries();
        public bool TryRead(in TKey k, [MaybeNullWhen(false)] out TData value);
        public bool TryStore(in TKey k, in TData value);
        public bool TryDelete(in TKey k);
    }
}
