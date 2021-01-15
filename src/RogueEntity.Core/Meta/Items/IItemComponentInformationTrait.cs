using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public interface IItemComponentInformationTrait<TItemId, TComponent> : IItemTrait 
        where TItemId : IEntityKey
    {
        bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out TComponent t);
    }
}