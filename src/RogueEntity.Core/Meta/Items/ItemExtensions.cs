using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;

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

        public static bool IsSameBulkType<TItemId>(this TItemId a, TItemId b)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return !a.IsEmpty && !a.IsReference && !b.IsReference && a.BulkItemId == b.BulkItemId;
        }
        
        public static bool IsSameType<TGameContext, TItemId>(this ItemResolver<TGameContext, TItemId> resolver, TItemId a, TItemId b)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            if (a.IsReference == false)
            {
                if (b.IsReference == false)
                {
                    return a.BulkItemId == b.BulkItemId;
                }

                return false;
            }

            if (b.IsReference == false)
            {
                return false;
            }

            return resolver.TryResolve(a, out var declA) &&
                   resolver.TryResolve(b, out var declB) &&
                   declA.Id == declB.Id;
        }
    }
}