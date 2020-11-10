using System;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemBuilder
{
    public static class ItemBuilderExtensions
    {
        public static ItemBuilder<TGameContext, TItemId> Build<TGameContext, TItemId>(this IItemResolver<TGameContext, TItemId> resolver,
                                                                                      TGameContext context,
                                                                                      ItemDeclarationId declaration)
            where TItemId : IEntityKey
        {
            if (resolver.ItemRegistry.TryGetItemById(declaration, out var itemDefinition))
            {
                var instantiated = resolver.Instantiate(context, itemDefinition);
                return new ItemBuilder<TGameContext, TItemId>(context, resolver, instantiated);
            }

            throw new ArgumentException("Invalid item declaration " + declaration);
        }
    }
}