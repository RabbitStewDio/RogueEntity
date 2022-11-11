using Microsoft.Extensions.ObjectPool;
using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;

public class SingleLevelPathPool
{
    readonly ObjectPool<SingleLevelPath> pool;

    public SingleLevelPathPool()
    {
        pool = new DefaultObjectPool<SingleLevelPath>(new Policy(this), 1024);
    }

    public SingleLevelPath Lease()
    {
        var lease = pool.Get();
        lease.Init();
        return lease;
    }

    public void Return(SingleLevelPath path)
    {
        pool.Return(path);
    }

    class Policy : IPooledObjectPolicy<SingleLevelPath>
    {
        readonly SingleLevelPathPool parentReference;

        public Policy(SingleLevelPathPool parentReference)
        {
            this.parentReference = parentReference ?? throw new ArgumentNullException(nameof(parentReference));
        }

        public SingleLevelPath Create()
        {
            return new SingleLevelPath(parentReference);
        }

        public bool Return(SingleLevelPath obj)
        {
            return true;
        }
    }
}