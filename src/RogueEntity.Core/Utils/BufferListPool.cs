using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Utils;

public static class BufferListPool<T>
{
    static readonly ObjectPool<BufferList<T>> pool = new DefaultObjectPool<BufferList<T>>(new BufferListObjectPoolPolicy<T>());
    static readonly IPooledObjectProvider<BufferList<T>> poolRef = new PoolRef();

    public static BufferList<T> Get()
    {
        return pool.Get();
    }

    public static void Return(BufferList<T> b)
    {
        pool.Return(b);
    }

    public static PooledObjectHandle<BufferList<T>> GetPooled()
    {
        return new PooledObjectHandle<BufferList<T>>(poolRef, pool.Get());
    }

    class PoolRef : IPooledObjectProvider<BufferList<T>>
    {
        public void Return(BufferList<T> t)
        {
            BufferListPool<T>.Return(t);;
        }
    }
}