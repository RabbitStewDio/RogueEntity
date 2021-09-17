using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Services;

namespace RogueEntity.Core.Meta.Items
{
    public static class ItemContextServices
    {
        public static IModuleContentContext<TEntityId> EnsureEntityRegistered<TEntityId>(this IModuleContentContext<TEntityId> ctx, IServiceResolver serviceResolver)
            where TEntityId : IEntityKey
        {
            if (EntityKeyMetaData.TryGetMetaData<TEntityId>(out var md))
            {
                serviceResolver.ConfigureEntityType(md);
            }
            else
            {
                throw new ModuleInitializationException();
            }
            return ctx;
        }

        public static IServiceResolver ConfigureEntityType<TEntity>(this IServiceResolver serviceResolver, IBulkDataStorageMetaData<TEntity> meta)
            where TEntity : IEntityKey
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
