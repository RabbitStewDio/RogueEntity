using EnTTSharp.Entities;

namespace RogueEntity.Api.ItemTraits
{
    public interface IReferenceItemDeclaration<TItemId> : IItemDeclaration where TItemId : IEntityKey
    {
        void Apply(IEntityViewControl<TItemId> v, TItemId k);
        void Initialize(IEntityViewControl<TItemId> v, TItemId k);
    }
}