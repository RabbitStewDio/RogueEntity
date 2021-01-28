using RogueEntity.Api.Services;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.DataViews;
using System;

namespace RogueEntity.Core.Chunks
{
    public static class PooledGridMapContextExtensions
    {
        public static PooledGridPositionContextBackend<TEntityId> GetOrCreatePooledGridMapContext<TEntityId>(this IServiceResolver serviceResolver)
        {
            return GetOrCreatePooledGridMapContext<TEntityId>(serviceResolver, PositionModuleServices.LookupDefaultConfiguration<TEntityId>(serviceResolver));
        }

        static IBoundedDataViewPool<TEntityId> GetOrCreateDataViewPool<TEntityId>(this IServiceResolver serviceResolver, DynamicDataViewConfiguration config)
        {
            if (serviceResolver.TryResolve(out IBoundedDataViewPool<TEntityId> pool))
            {
                return pool;
            }

            pool = new DefaultBoundedDataViewPool<TEntityId>(config);
            serviceResolver.Store(pool);
            return pool;
        }
        
        public static PooledGridPositionContextBackend<TEntityId> GetOrCreatePooledGridMapContext<TEntityId>(this IServiceResolver serviceResolver, DynamicDataViewConfiguration config)
        {
            if (serviceResolver.TryResolve(out IGridMapContext<TEntityId> maybeMap))
            {
                if (maybeMap is PooledGridPositionContextBackend<TEntityId> defaultImpl)
                {
                    return defaultImpl;
                }

                throw new InvalidOperationException("A conflicting grid map implementation has been defined.");
            }

            var pool = serviceResolver.GetOrCreateDataViewPool<TEntityId>(config);
            var created = new PooledGridPositionContextBackend<TEntityId>(pool);
            if (!serviceResolver.TryResolve(out IGridMapConfiguration<TEntityId> _))
            {
                serviceResolver.Store<IGridMapConfiguration<TEntityId>>(new GridMapConfiguration<TEntityId>(config));
            }

            serviceResolver.Store<IGridMapContext<TEntityId>>(created);
            return created;
        }


    }
}
