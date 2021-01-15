using EnTTSharp.Entities;

namespace RogueEntity.Api.ItemTraits
{
    public interface IItemContext<TItemId> 
        where TItemId : IEntityKey
    {
        IItemResolver<TItemId> ItemResolver { get; }
    }
    
    public interface IItemContextBackend<TItemId>: IItemContext<TItemId>
        where TItemId : IEntityKey
    {
        IBulkDataStorageMetaData<TItemId> EntityMetaData { get; }
        IItemRegistryBackend<TItemId> ItemRegistry { get; }
        EntityRegistry<TItemId> EntityRegistry { get; }
    }
}