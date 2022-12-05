using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Utils;

public static class BufferListPool<T>
{
    static readonly ObjectPool<BufferList<T>> pool = new DefaultObjectPool<BufferList<T>>(new BufferListObjectPoolPolicy<T>());

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
        return new PooledObjectHandle<BufferList<T>>(pool, pool.Get());
    }
}