using EnTTSharp.Entities;

namespace RogueEntity.Core.Meta.Items
{
    public interface IItemComponentTrait<TItemId, TComponent> : IItemComponentInformationTrait<TItemId, TComponent>
        where TItemId : struct, IEntityKey
    {
        bool TryUpdate(IEntityViewControl<TItemId> v, TItemId k, in TComponent t, out TItemId changedK);
        bool TryRemove(IEntityViewControl<TItemId> v, TItemId k, out TItemId changedK);
    }
}