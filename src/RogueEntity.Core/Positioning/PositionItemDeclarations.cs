using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Positioning
{
    public static class PositionItemDeclarations
    {
        public static BulkItemDeclarationBuilder<TGameContext, TItemId> AsImmobile<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new ImmobilityMarkerTrait<TGameContext, TItemId>());
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> AsImmobile<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new ImmobilityMarkerTrait<TGameContext, TItemId>());
            return builder;
        }
        
    }
}