using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.ItemBuilder
{
    public sealed class ItemBuilder<TItemId> : ItemBuilderBase<TItemId, ItemBuilder<TItemId>>
        where TItemId : IEntityKey
    {
        public ItemBuilder(IItemResolver<TItemId> resolver,
                           TItemId reference) : base(resolver, reference)
        {
        }
    }
}