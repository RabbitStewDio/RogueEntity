using EnttSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Meta.Items
{
    public interface IReferenceItemDeclaration<TContext, TItemId> : IItemDeclaration where TItemId : IEntityKey
    {
        void Apply(IEntityViewControl<TItemId> v, TContext context, TItemId k);
        void Initialize(IEntityViewControl<TItemId> v, TContext context, TItemId k);
    }
}