using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning.Grid
{
    public static class GridPositionItemDeclarations
    {
        public static BulkItemDeclarationBuilder<TItemId> WithGridPosition<TItemId>(this BulkItemDeclarationBuilder<TItemId> builder, 
                                                                                    BodySize bodySize,
                                                                                    MapLayer layer, params MapLayer[] layers)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new BulkItemGridPositionTrait<TItemId>(bodySize, layer, layers));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TItemId> WithGridPosition<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder, 
                                                                                         BodySize bodySize,
                                                                                         MapLayer layer, params MapLayer[] layers)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new ReferenceItemGridPositionTrait<TItemId>(bodySize, layer, layers));
            return builder;
        }
    }
}
