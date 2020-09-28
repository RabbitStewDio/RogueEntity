using EnTTSharp.Entities;

namespace RogueEntity.Core.Meta.Items
{
    public interface IItemComponentInformationTrait<in TContext, TItemId, TComponent> : IItemTrait 
        where TItemId : IEntityKey
    {
        bool TryQuery(IEntityViewControl<TItemId> v, TContext context, TItemId k, out TComponent t);
    }
}