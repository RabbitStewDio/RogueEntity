using System;
using EnTTSharp.Entities;
using MessagePack;
using MessagePack.Formatters;
using RogueEntity.Core.Infrastructure.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemDeclarationHolderMessagePackFormatter<TContext, TItemId> : IMessagePackFormatter<ItemDeclarationHolder<TContext, TItemId>> 
        where TItemId : IEntityKey
        where TContext: IItemContext<TContext, TItemId>
    {
        readonly TContext context;

        public ItemDeclarationHolderMessagePackFormatter(TContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Serialize(ref MessagePackWriter writer, ItemDeclarationHolder<TContext, TItemId> value, MessagePackSerializerOptions options)
        {
            writer.Write(value.Id);
        }

        public ItemDeclarationHolder<TContext, TItemId> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var id = reader.ReadString();
            var itemRaw = context.ItemResolver.ItemRegistry.ReferenceItemById(id);
            if (itemRaw is IReferenceItemDeclaration<TContext, TItemId> item)
            {
                return new ItemDeclarationHolder<TContext, TItemId>(item);
            }

            throw new MessagePackSerializationException($"Unable to resolve reference item with id '{id}'");
        }
    }
}