using System.Diagnostics.CodeAnalysis;
using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.ItemTraits
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Needed as discriminator")]
    public interface IItemRegistryBackend<TContext, TItemId>: IItemRegistry, IBulkItemIdMapping
        where TItemId : IEntityKey
    {
        ItemDeclarationId Register(IItemDeclaration itemDeclaration);
    }
}