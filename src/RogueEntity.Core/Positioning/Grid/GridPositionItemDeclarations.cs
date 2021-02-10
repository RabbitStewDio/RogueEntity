using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning.Grid
{
    public static class GridPositionItemDeclarations
    {
        public static BulkItemDeclarationBuilder<TItemId> WithGridPosition<TItemId>(this BulkItemDeclarationBuilder<TItemId> builder, MapLayer layer, params MapLayer[] layers)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new BulkItemGridPositionTrait<TItemId>(layer, layers));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TItemId> WithGridPosition<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder, MapLayer layer, params MapLayer[] layers)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new ReferenceItemGridPositionTrait<TItemId>(layer, layers));
            return builder;
        }
    }
}
