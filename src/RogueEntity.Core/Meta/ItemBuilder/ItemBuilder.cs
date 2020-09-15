using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemBuilder
{
    public sealed class ItemBuilder<TGameContext, TItemId> : ItemBuilderBase<TGameContext, TItemId, ItemBuilder<TGameContext, TItemId>>
        where TItemId : IEntityKey
    {
        public ItemBuilder(TGameContext context,
                           IItemResolver<TGameContext, TItemId> resolver,
                           TItemId reference) : base(context, resolver, reference)
        {
        }
    }
}