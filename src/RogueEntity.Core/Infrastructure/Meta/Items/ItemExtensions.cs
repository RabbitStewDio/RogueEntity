namespace RogueEntity.Core.Infrastructure.Meta.Items
{
    public static class ItemExtensions
    {
        public static bool TryQueryDataTrait<TData>(this IItemDeclaration item, out TData data)
        {
            if (item.TryQuery(out IItemComponentInformationTrait<TData> trait))
            {
                data = trait.BaseValue;
                return true;
            }

            data = default;
            return false;
        }
    }
}