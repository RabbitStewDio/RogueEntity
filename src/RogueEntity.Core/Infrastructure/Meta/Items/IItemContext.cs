using EnttSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Meta.Items
{
    public interface IItemContext<TGameContext, TItemId> 
        where TItemId : IEntityKey
    {
        IItemResolver<TGameContext, TItemId> ItemResolver { get; }
    }
}