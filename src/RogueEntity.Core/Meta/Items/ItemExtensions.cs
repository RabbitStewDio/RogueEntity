using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public static class ItemExtensions
    {
        public static bool HasItemComponent<TItemId, TData>(this IItemDeclaration item)
            where TItemId : IEntityKey
        {
            return item.TryQuery(out IItemComponentInformationTrait<TItemId, TData> _);
        }

        public static TItemId Instantiate<TItemId>(this IItemResolver<TItemId> resolver,
                                                   ItemDeclarationId id)
            where TItemId : IEntityKey

        {
            var item = resolver.ItemRegistry.ReferenceItemById(id);
            return resolver.Instantiate(item);
        }
    }
}
