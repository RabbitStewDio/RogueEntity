using EnTTSharp.Entities;

namespace RogueEntity.Core.Meta.Items
{
    public interface IItemContext<TGameContext, TItemId> 
        where TItemId : IEntityKey
    {
        IItemResolver<TGameContext, TItemId> ItemResolver { get; }
    }
}