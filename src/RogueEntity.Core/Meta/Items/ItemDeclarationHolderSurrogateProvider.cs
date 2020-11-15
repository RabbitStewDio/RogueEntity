using System;
using EnTTSharp.Entities;
using EnTTSharp.Serialization.Xml;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemDeclarationHolderSurrogateProvider<TGameContext, TItemId>: SerializationSurrogateProviderBase<ItemDeclarationHolder<TGameContext, TItemId>, SurrogateContainer<string>> 
        where TItemId : IEntityKey
    {
        readonly IItemResolver<TGameContext, TItemId> itemResolver;

        public ItemDeclarationHolderSurrogateProvider([NotNull] IItemResolver<TGameContext, TItemId> itemResolver)
        {
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
        }

        public override ItemDeclarationHolder<TGameContext, TItemId> GetDeserializedObject(SurrogateContainer<string> surr)
        {
            var id = surr.Content;
            var itemRaw = itemResolver.ItemRegistry.ReferenceItemById(id);
            if (itemRaw is IReferenceItemDeclaration<TGameContext, TItemId> item)
            {
                return new ItemDeclarationHolder<TGameContext, TItemId>(item);
            }

            throw new SurrogateResolverException($"Unable to resolve reference item with id '{id}'");
        }

        public override SurrogateContainer<string> GetObjectToSerialize(ItemDeclarationHolder<TGameContext, TItemId> obj)
        {
            return new SurrogateContainer<string>(obj.Id);
        }
    }
}