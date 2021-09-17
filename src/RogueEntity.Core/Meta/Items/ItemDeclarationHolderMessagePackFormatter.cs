using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using MessagePack;
using MessagePack.Formatters;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemDeclarationHolderMessagePackFormatter<TItemId> : IMessagePackFormatter<ItemDeclarationHolder<TItemId>> 
        where TItemId : IEntityKey
    {
        readonly IItemResolver<TItemId> itemResolver;

        public ItemDeclarationHolderMessagePackFormatter([NotNull] IItemResolver<TItemId> itemResolver)
        {
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
        }

        public void Serialize(ref MessagePackWriter writer, ItemDeclarationHolder<TItemId> value, MessagePackSerializerOptions options)
        {
            writer.Write(value.ItemId);
        }

        public ItemDeclarationHolder<TItemId> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var id = reader.ReadString();
            var itemRaw = itemResolver.ItemRegistry.ReferenceItemById(id);
            if (itemRaw is IReferenceItemDeclaration<TItemId> item)
            {
                return new ItemDeclarationHolder<TItemId>(item);
            }

            throw new MessagePackSerializationException($"Unable to resolve reference item with id '{id}'");
        }
    }
}