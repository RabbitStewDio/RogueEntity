using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Meta.Items
{
    public interface IItemComponentInformationTrait<TItemId, TComponent> : IItemTrait 
        where TItemId : struct, IEntityKey
    {
        bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, [MaybeNullWhen(false)] out TComponent t);
    }
}