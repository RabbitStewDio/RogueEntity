using EnTTSharp.Entities;

namespace RogueEntity.Core.Meta.Items
{
    public interface IItemComponentTrait<in TContext, TItemId, TComponent> : IItemComponentInformationTrait<TContext, TItemId, TComponent>
        where TItemId : IEntityKey
    {
        bool TryUpdate(IEntityViewControl<TItemId> v, TContext context, TItemId k, in TComponent t, out TItemId changedK);
        bool TryRemove(IEntityViewControl<TItemId> v, TContext context, TItemId k, out TItemId changedK);
    }
}