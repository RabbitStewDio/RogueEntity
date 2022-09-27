using EnTTSharp.Entities;

namespace RogueEntity.Api.ItemTraits
{
    public interface IReferenceItemDeclaration<TItemId> : IItemDeclaration where TItemId : struct, IEntityKey
    {
        void Apply(IEntityViewControl<TItemId> v, TItemId k);
        void Initialize(IEntityViewControl<TItemId> v, TItemId k);
        IReferenceItemDeclaration<TItemId> WithTrait(IReferenceItemTrait<TItemId> trait);
        IReferenceItemDeclaration<TItemId> WithoutTrait<TTrait>();
    }
}