using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning.Grid
{
    public static class GridPositionItemDeclarations
    {
        public static BulkItemDeclarationBuilder<TItemId> WithGridPosition<TItemId>(this BulkItemDeclarationBuilder<TItemId> builder, MapLayer layer, params MapLayer[] layers)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            var itemMeta = builder.ServiceResolver.Resolve<IBulkDataStorageMetaData<TItemId>>();
            var itemCtx = builder.ServiceResolver.Resolve<IItemResolver<TItemId>>();
            var gridCtx = builder.ServiceResolver.Resolve<IGridMapContext<TItemId>>();
            builder.Declaration.WithTrait(new BulkItemGridPositionTrait<TItemId>(itemMeta, itemCtx, gridCtx, layer, layers));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TItemId> WithGridPosition<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder, MapLayer layer, params MapLayer[] layers)
            where TItemId : IEntityKey
        {
            var itemCtx = builder.ServiceResolver.Resolve<IItemResolver<TItemId>>();
            var gridCtx = builder.ServiceResolver.Resolve<IGridMapContext<TItemId>>();
            builder.Declaration.WithTrait(new ReferenceItemGridPositionTrait<TItemId>(itemCtx, gridCtx, layer, layers));
            return builder;
        }
    }
}
