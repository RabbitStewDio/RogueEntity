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

        public static bool IsSameStackType<TEntity>(this IItemResolver<TEntity> r, TEntity a, TEntity b)
            where TEntity : struct, IEntityKey
        {
            if (a.IsEmpty || b.IsEmpty) return false;
            if (r.TryResolve(a, out var itemDeclarationA) &&
                r.TryResolve(b, out var itemDeclarationB))
            {
                return itemDeclarationA.Id == itemDeclarationB.Id;
            }                    

            return false;
        } 
        
    }
}
