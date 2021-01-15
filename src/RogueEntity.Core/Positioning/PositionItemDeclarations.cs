using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Positioning
{
    public static class PositionItemDeclarations
    {
        public static BulkItemDeclarationBuilder< TItemId> AsImmobile< TItemId>(this BulkItemDeclarationBuilder< TItemId> builder)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new ImmobilityMarkerTrait< TItemId>());
            return builder;
        }

        public static ReferenceItemDeclarationBuilder< TItemId> AsImmobile< TItemId>(this ReferenceItemDeclarationBuilder< TItemId> builder)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new ImmobilityMarkerTrait< TItemId>());
            return builder;
        }
        
    }
}