using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using MessagePack;
using MessagePack.Formatters;
using RogueEntity.Core.Infrastructure.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemDeclarationHolderMessagePackFormatter<TGameContext, TItemId> : IMessagePackFormatter<ItemDeclarationHolder<TGameContext, TItemId>> 
        where TItemId : IEntityKey
    {
        readonly IItemResolver<TGameContext, TItemId> itemResolver;

        public ItemDeclarationHolderMessagePackFormatter([NotNull] IItemResolver<TGameContext, TItemId> itemResolver)
        {
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
        }

        public void Serialize(ref MessagePackWriter writer, ItemDeclarationHolder<TGameContext, TItemId> value, MessagePackSerializerOptions options)
        {
            writer.Write(value.Id);
        }

        public ItemDeclarationHolder<TGameContext, TItemId> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var id = reader.ReadString();
            var itemRaw = itemResolver.ItemRegistry.ReferenceItemById(id);
            if (itemRaw is IReferenceItemDeclaration<TGameContext, TItemId> item)
            {
                return new ItemDeclarationHolder<TGameContext, TItemId>(item);
            }

            throw new MessagePackSerializationException($"Unable to resolve reference item with id '{id}'");
        }
    }
}