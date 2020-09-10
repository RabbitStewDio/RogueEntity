using System;
using EnTTSharp.Entities;
using EnTTSharp.Serialization.Xml;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemDeclarationHolderSurrogateProvider<TGameContext, TItemId>: SerializationSurrogateProviderBase<ItemDeclarationHolder<TGameContext, TItemId>, SurrogateContainer<string>> 
        where TItemId : IEntityKey
        where TGameContext: IItemContext<TGameContext, TItemId>
    {
        readonly TGameContext context;

        public ItemDeclarationHolderSurrogateProvider(TGameContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override ItemDeclarationHolder<TGameContext, TItemId> GetDeserializedObject(SurrogateContainer<string> surr)
        {
            var id = surr.Content;
            var itemRaw = context.ItemResolver.ItemRegistry.ReferenceItemById(id);
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