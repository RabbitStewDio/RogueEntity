using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.ItemTraits
{
    public interface IItemContext<TGameContext, TItemId> 
        where TItemId : IEntityKey
    {
        IItemResolver<TGameContext, TItemId> ItemResolver { get; }
    }
    
    public interface IItemContextBackend<TGameContext, TItemId>: IItemContext<TGameContext, TItemId>
        where TItemId : IEntityKey
    {
        IItemRegistryBackend<TGameContext, TItemId> ItemRegistry { get; }
        EntityRegistry<TItemId> EntityRegistry { get; }
    }
}