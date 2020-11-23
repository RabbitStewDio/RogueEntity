using System.Collections.Generic;

namespace RogueEntity.Api.ItemTraits
{
    /// <summary>
    ///  A small helper interface that allows queries of all declared bulk item declarations.
    ///  Used during serialization and deserialization.
    /// </summary>
    public interface IBulkItemIdMapping: IEnumerable<int>
    {
        bool TryResolveBulkItem(int id, out ItemDeclarationId itemName);
    }
}