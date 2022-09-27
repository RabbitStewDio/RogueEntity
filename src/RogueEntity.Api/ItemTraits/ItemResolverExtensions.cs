using EnTTSharp.Entities;

namespace RogueEntity.Api.ItemTraits
{
    public static class ItemResolverExtensions
    {
        public static bool IsItemType<TEntity>(this IItemResolver<TEntity> r, TEntity k, ItemDeclarationId d)
            where TEntity : struct, IEntityKey
        {
            return r.TryResolve(k, out var id) && id.Id == d;
        }

        public static WorldEntityTag QueryItemTag<TEntity>(this IItemResolver<TEntity> r, TEntity k)
            where TEntity : struct, IEntityKey
        {
            if (r.TryResolve(k, out var id))
            {
                return id.Tag;
            }

            return default;
        }
        
        public static ItemDeclarationId QueryItemId<TEntity>(this IItemResolver<TEntity> r, TEntity k)
            where TEntity : struct, IEntityKey
        {
            if (r.TryResolve(k, out var id))
            {
                return id.Id;
            }

            return default;
        }

    }
}
