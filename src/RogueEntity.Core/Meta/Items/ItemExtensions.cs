using EnTTSharp.Entities;

namespace RogueEntity.Core.Meta.Items
{
    public static class ItemExtensions
    {
        public static bool TryQueryInformationTrait<TData>(this IItemDeclaration item, out TData data)
        {
            if (item.TryQuery(out IItemComponentInformationTrait<TData> trait))
            {
                data = trait.BaseValue;
                return true;
            }

            data = default;
            return false;
        }

        public static bool HasItemComponent<TGameContext, TItemId, TData>(this IItemDeclaration item) where TItemId : IEntityKey
        {
            return item.TryQuery(out IItemComponentTrait<TGameContext, TItemId, TData> _);
        }
    }
}