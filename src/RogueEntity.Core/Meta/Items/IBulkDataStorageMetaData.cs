using System;
using EnTTSharp.Entities;

namespace RogueEntity.Core.Meta.Items
{
    public interface IBulkDataStorageMetaData<TItemId> where TItemId : IEntityKey
    {
        public int MaxAge { get; }
        public Func<byte, int, TItemId> EntityKeyFactory { get; }
        public Func<int, TItemId> BulkDataFactory { get; }
    }
}