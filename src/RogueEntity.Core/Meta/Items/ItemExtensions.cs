using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public static class ItemExtensions
    {
        public static bool HasItemComponent<TItemId, TData>(this IItemDeclaration item)
            where TItemId : struct, IEntityKey
        {
            return item.TryQuery<IItemComponentInformationTrait<TItemId, TData>>(out _);
        }

        public static TItemId Instantiate<TItemId>(this IItemResolver<TItemId> resolver,
                                                   ItemDeclarationId id)
            where TItemId : struct, IEntityKey

        {
            var item = resolver.ItemRegistry.ReferenceItemById(id);
            return resolver.Instantiate(item);
        }
    }
}
