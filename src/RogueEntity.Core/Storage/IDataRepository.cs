using System.Collections.Generic;

namespace RogueEntity.Core.Storage
{
    public interface IDataRepository<TKey, TData>
    {
        public IEnumerable<TKey> QueryEntries();
        public bool TryRead(in TKey k, out TData value);
        public bool TryStore(in TKey k, in TData value);
        public bool TryDelete(in TKey k);
    }
}
