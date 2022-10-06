using System;

namespace RogueEntity.Core.Utils
{
    public interface IPooledObjectProvider<T>
    {
        void Return(T t);
    }

    public readonly struct PooledObjectHandle<T>: IDisposable
    {
        readonly IPooledObjectProvider<T> pool;
        public readonly T Data;

        public PooledObjectHandle(IPooledObjectProvider<T> pool,
                                  T data)
        {
            this.pool = pool ?? throw new ArgumentNullException(nameof(pool));
            this.Data = data;
        }

        public void Dispose()
        {
            this.pool.Return(Data);
        }

        public static implicit operator T(PooledObjectHandle<T> h) => h.Data;
    }
}