using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public interface IReferenceItemDeclaration<TContext, TItemId> : IItemDeclaration where TItemId : IEntityKey
    {
        void Apply(IEntityViewControl<TItemId> v, TContext context, TItemId k);
        void Initialize(IEntityViewControl<TItemId> v, TContext context, TItemId k);
    }
}