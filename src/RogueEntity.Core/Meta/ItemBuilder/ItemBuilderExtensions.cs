using System;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.ItemBuilder
{
    public static class ItemBuilderExtensions
    {
        public static ItemBuilder<TItemId> Build<TItemId>(this IItemResolver<TItemId> resolver,
                                                          ItemDeclarationId declaration)
            where TItemId : IEntityKey
        {
            if (resolver.ItemRegistry.TryGetItemById(declaration, out var itemDefinition))
            {
                var instantiated = resolver.Instantiate(itemDefinition);
                return new ItemBuilder<TItemId>(resolver, instantiated);
            }

            throw new ArgumentException("Invalid item declaration " + declaration);
        }
    }
}
