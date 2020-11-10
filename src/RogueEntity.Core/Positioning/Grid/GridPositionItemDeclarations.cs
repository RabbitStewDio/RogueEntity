using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning.Grid
{
    public static class GridPositionItemDeclarations
    {
        public static BulkItemDeclarationBuilder<TGameContext, TItemId> WithGridPosition<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder, MapLayer layer, params MapLayer[] layers)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            var itemCtx = builder.ServiceResolver.Resolve<IItemContext<TGameContext, TItemId>>();
            var gridCtx = builder.ServiceResolver.Resolve<IGridMapContext<TItemId>>();
            builder.Declaration.WithTrait(new BulkItemGridPositionTrait<TGameContext, TItemId>(itemCtx.ItemResolver, gridCtx, layer, layers));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithGridPosition<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder, MapLayer layer, params MapLayer[] layers)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            var itemCtx = builder.ServiceResolver.Resolve<IItemContext<TGameContext, TItemId>>();
            var gridCtx = builder.ServiceResolver.Resolve<IGridMapContext<TItemId>>();
            builder.Declaration.WithTrait(new ReferenceItemGridPositionTrait<TGameContext, TItemId>(itemCtx.ItemResolver, gridCtx, layer, layers));
            return builder;
        }
        
        public static BulkItemDeclarationBuilder<TGameContext, TItemId> AsImmobile<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            builder.Declaration.WithTrait(new ImmobilityMarkerTrait<TGameContext, TItemId>());
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> AsImmobile<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            builder.Declaration.WithTrait(new ImmobilityMarkerTrait<TGameContext, TItemId>());
            return builder;
        }
        
        

    }
}