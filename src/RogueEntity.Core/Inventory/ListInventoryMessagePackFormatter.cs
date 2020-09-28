using System;
using EnTTSharp.Entities;
using MessagePack;
using MessagePack.Formatters;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Inventory
{
    public class ListInventoryMessagePackFormatter<TGameContext, TOwnerId, TItemId> : IMessagePackFormatter<ListInventory<TGameContext, TOwnerId, TItemId>>
        where TItemId : IBulkDataStorageKey<TItemId>
        where TOwnerId : IEntityKey
    {
        readonly IItemResolver<TGameContext, TItemId> itemResolver;

        public ListInventoryMessagePackFormatter(IItemResolver<TGameContext, TItemId> itemResolver)
        {
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
        }

        public void Serialize(ref MessagePackWriter writer, ListInventory<TGameContext, TOwnerId, TItemId> value, MessagePackSerializerOptions options)
        {
            MessagePackSerializer.Serialize(ref writer, value.Data, options);
        }

        public ListInventory<TGameContext, TOwnerId, TItemId> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var data = MessagePackSerializer.Deserialize<ListInventoryData<TOwnerId, TItemId>>(ref reader, options);
            return new ListInventory<TGameContext, TOwnerId, TItemId>(itemResolver, data);
        }
    }
}