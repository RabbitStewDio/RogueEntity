namespace RogueEntity.Core.Infrastructure.Meta.Items
{
    public static class BulkDataStorageKeyExtensions
    {
        public static bool IsSameBulkDataType<TItemId>(this TItemId r, TItemId t)
            where TItemId: IBulkDataStorageKey<TItemId>
        {
            if (r.IsReference || t.IsReference)
            {
                return false;
            }
            return (r.BulkItemId == t.BulkItemId);
        }
    }
}