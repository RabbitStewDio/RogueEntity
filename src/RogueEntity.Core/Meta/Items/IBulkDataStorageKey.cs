using EnTTSharp.Entities;

namespace RogueEntity.Core.Meta.Items
{
    public interface IBulkDataStorageKey<TSelf>: IEntityKey 
        where TSelf: IBulkDataStorageKey<TSelf>
    {
        bool IsEmpty { get; }
        bool IsReference { get; }
        int BulkItemId { get; }

        TSelf WithData(int data);
        int Data { get; }
    }
}