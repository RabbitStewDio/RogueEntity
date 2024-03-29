﻿using System;
using EnTTSharp.Entities;
using EnTTSharp.Serialization.Xml;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemDeclarationHolderSurrogateProvider<TItemId>: SerializationSurrogateProviderBase<ItemDeclarationHolder<TItemId>, SurrogateContainer<string>> 
        where TItemId : struct, IEntityKey
    {
        readonly IItemResolver<TItemId> itemResolver;

        public ItemDeclarationHolderSurrogateProvider(IItemResolver<TItemId> itemResolver)
        {
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
        }

        public override ItemDeclarationHolder<TItemId> GetDeserializedObject(SurrogateContainer<string> surr)
        {
            var id = surr.Content;
            var itemRaw = itemResolver.ItemRegistry.ReferenceItemById(id);
            if (itemRaw is IReferenceItemDeclaration<TItemId> item)
            {
                return new ItemDeclarationHolder<TItemId>(item);
            }

            throw new SurrogateResolverException($"Unable to resolve reference item with id '{id}'");
        }

        public override SurrogateContainer<string> GetObjectToSerialize(ItemDeclarationHolder<TItemId> obj)
        {
            return new SurrogateContainer<string>(obj.ItemId);
        }
    }
}