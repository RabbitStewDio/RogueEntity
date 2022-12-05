using Microsoft.Extensions.ObjectPool;

namespace RogueEntity.Core.Utils;

public static class PoolExtensions
{
    public static PooledObjectHandle<T> GetPooled<T>(this ObjectPool<T> pool)
        where T: class
    {
        return new PooledObjectHandle<T>(pool, pool.Get());
    }

    public static IPooledObjectPolicy<TBaseType> DownGrade<TBaseType, TType>(this IPooledObjectPolicy<TType> p)
        where TType: class, TBaseType
        where TBaseType: class
    {
        return new DowngradeObjectPolicy<TBaseType, TType>(p);
    }

    class DowngradeObjectPolicy<TBaseType, TType> : IPooledObjectPolicy<TBaseType>
        where TType: class, TBaseType
        where TBaseType: class
    {
        readonly IPooledObjectPolicy<TType> pool;

        public DowngradeObjectPolicy(IPooledObjectPolicy<TType> pool)
        {
            this.pool = pool;
        }

        public TBaseType Create()
        {
            return pool.Create();
        }

        public bool Return(TBaseType obj)
        {
            if (obj is TType typedObj)
            {
                return pool.Return(typedObj);
            }

            return false;
        }
    }

}