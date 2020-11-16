using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public static class ItemExtensions
    {
        public static bool HasItemComponent<TGameContext, TItemId, TData>(this IItemDeclaration item)
            where TItemId : IEntityKey
        {
            return item.TryQuery(out IItemComponentInformationTrait<TGameContext, TItemId, TData> _);
        }

        public static TItemId Instantiate<TGameContext, TItemId>(this IItemResolver<TGameContext, TItemId> resolver,
                                                                 TGameContext context,
                                                                 ItemDeclarationId id)
            where TItemId : IEntityKey

        {
            var item = resolver.ItemRegistry.ReferenceItemById(id);
            return resolver.Instantiate(context, item);
        }
    }
}