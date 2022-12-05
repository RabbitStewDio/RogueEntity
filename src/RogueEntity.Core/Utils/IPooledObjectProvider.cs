using Microsoft.Extensions.ObjectPool;
using System;

namespace RogueEntity.Core.Utils
{
    public readonly struct PooledObjectHandle<T> : IDisposable
        where T : class
    {
        readonly ObjectPool<T> pool;
        public readonly T Data;

        public PooledObjectHandle(ObjectPool<T> pool,
                                  T data)
        {
            this.pool = pool ?? throw new ArgumentNullException(nameof(pool));
            this.Data = data;
        }

        public void Dispose()
        {
            this.pool?.Return(Data);
        }

        public static implicit operator T(PooledObjectHandle<T> h) => h.Data;
    }
}