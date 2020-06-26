using EnttSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Meta.Items
{
    public interface IItemComponentTrait<in TContext, TItemId, TComponent> : IItemTrait 
        where TItemId : IEntityKey
    {
        bool TryQuery(IEntityViewControl<TItemId> v, TContext context, TItemId k, out TComponent t);
        bool TryUpdate(IEntityViewControl<TItemId> v, TContext context, TItemId k, in TComponent t, out TItemId changedK);
    }
}