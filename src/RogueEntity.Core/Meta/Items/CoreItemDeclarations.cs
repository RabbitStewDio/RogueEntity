using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Meta.Items
{
    public static class CoreItemDeclarations
    {
        public static BulkItemDeclarationBuilder<TItemId> WithRole<TItemId>(this BulkItemDeclarationBuilder<TItemId> builder, EntityRole layer, params EntityRole[] layers)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new RoleDefinitionTrait<TItemId>(layer, layers));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TItemId> WithRole<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder, EntityRole layer, params EntityRole[] layers)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new RoleDefinitionTrait<TItemId>(layer, layers));
            return builder;
        }

    }
}
