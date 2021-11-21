using EnTTSharp.Entities;

namespace RogueEntity.Api.ItemTraits
{
    public interface IBulkItemDeclaration<TItemId> : IItemDeclaration
        where TItemId: IEntityKey
    {
        TItemId Initialize(TItemId itemReference);
        IBulkItemDeclaration<TItemId> WithTrait(IBulkItemTrait<TItemId> trait);
        IBulkItemDeclaration<TItemId> WithoutTrait<TTrait>();
    }
}