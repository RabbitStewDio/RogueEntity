using EnTTSharp.Entities;

namespace RogueEntity.Api.ItemTraits
{
    public interface IBulkDataStorageKey<TSelf>: IEntityKey where TSelf: IBulkDataStorageKey<TSelf>
    {
        bool IsReference { get; }
        int BulkItemId { get; }

        TSelf WithData(int data);
        int Data { get; }
    }
}