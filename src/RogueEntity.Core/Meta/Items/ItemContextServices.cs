using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Services;

namespace RogueEntity.Core.Meta.Items
{
    public static class ItemContextServices
    {
        public static IServiceResolver ConfigureEntityType<TGameContext, TEntity>(this IServiceResolver serviceResolver, IBulkDataStorageMetaData<TEntity> meta)
            where TEntity : IBulkDataStorageKey<TEntity>
        {
            if (!serviceResolver.TryResolve(out IItemContextBackend<TGameContext, TEntity> actorBackend))
            {
                actorBackend = new ItemContextBackend<TGameContext, TEntity>(meta);
                serviceResolver.Store(actorBackend);
                serviceResolver.Store(actorBackend.ItemResolver);
                serviceResolver.Store(actorBackend.EntityMetaData);
            }

            if (!serviceResolver.TryResolve(out IItemResolver<TGameContext, TEntity> _))
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
