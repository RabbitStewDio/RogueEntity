using EnTTSharp.Entities;

namespace RogueEntity.Api.ItemTraits
{
    public interface IBulkItemDeclaration<TItemId> : IItemDeclaration
        where TItemId: IEntityKey
    {
        TItemId Initialize(TItemId itemReference);
    }
}