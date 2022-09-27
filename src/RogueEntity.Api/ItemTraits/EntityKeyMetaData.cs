using EnTTSharp.Entities;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace RogueEntity.Api.ItemTraits
{
    public static class EntityKeyMetaData
    {
        public static bool TryGetMetaData<TEntityKey>([MaybeNullWhen(false)]out IBulkDataStorageMetaData<TEntityKey> meta)
            where TEntityKey : struct, IEntityKey
        {
            var attr = typeof(TEntityKey).GetCustomAttribute<EntityKeyMetaDataAttribute>();
            if (attr == null)
            {
                meta = default;
                return false;
            }

            var expectedType = attr.MetaData;
            var field = expectedType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
            var maybeFieldValue = field?.GetValue(null);
            if (maybeFieldValue is IBulkDataStorageMetaData<TEntityKey> m)
            {
                meta = m;
                return true;
            }

            var prop = expectedType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            var maybePropValue = prop?.GetValue(null);
            if (maybePropValue is IBulkDataStorageMetaData<TEntityKey> mx)
            {
                meta = mx;
                return true;
            }

            meta = default;
            return false;
        }
    }
}
