using EnTTSharp.Entities;

namespace RogueEntity.Api.ItemTraits
{
    public static class ItemResolverExtensions
    {
        public static bool IsItemType<TEntity>(this IItemResolver<TEntity> r, TEntity k, ItemDeclarationId d)
            where TEntity : IEntityKey
        {
            return r.TryResolve(k, out var id) && id.Id == d;
        }
    }
}
