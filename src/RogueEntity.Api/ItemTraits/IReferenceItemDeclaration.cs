using EnTTSharp.Entities;

namespace RogueEntity.Api.ItemTraits
{
    public interface IReferenceItemDeclaration<TContext, TItemId> : IItemDeclaration where TItemId : IEntityKey
    {
        void Apply(IEntityViewControl<TItemId> v, TContext context, TItemId k);
        void Initialize(IEntityViewControl<TItemId> v, TContext context, TItemId k);
    }
}