using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Services;

namespace RogueEntity.Core.Meta.Items
{
    public static class ItemContextServices
    {
        public static IServiceResolver ConfigureEntityType<TEntity>(this IServiceResolver serviceResolver, IBulkDataStorageMetaData<TEntity> meta)
            where TEntity : IBulkDataStorageKey<TEntity>
        {
            if (!serviceResolver.TryResolve(out IItemContextBackend<TEntity> actorBackend))
            {
                actorBackend = new ItemContextBackend<TEntity>(meta);
                serviceResolver.Store(actorBackend);
                serviceResolver.Store(actorBackend.ItemResolver);
                serviceResolver.Store(actorBackend.EntityMetaData);
            }

            if (!serviceResolver.TryResolve(out IItemResolver<TEntity> _))
            {
                serviceResolver.Store(actorBackend.ItemResolver);
            }

            if (!serviceResolver.TryResolve(out IBulkDataStorageMetaData<TEntity> _))
            {
                serviceResolver.Store(actorBackend.EntityMetaData);
            }


            return serviceResolver;
        }
    }
}
