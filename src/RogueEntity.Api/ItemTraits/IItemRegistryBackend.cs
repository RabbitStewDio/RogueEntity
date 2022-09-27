using System.Diagnostics.CodeAnalysis;
using EnTTSharp.Entities;

namespace RogueEntity.Api.ItemTraits
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Needed as discriminator")]
    public interface IItemRegistryBackend<TItemId>: IItemRegistry, IBulkItemIdMapping
        where TItemId : struct, IEntityKey
    {
        ItemDeclarationId Register(IItemDeclaration itemDeclaration);
    }
}