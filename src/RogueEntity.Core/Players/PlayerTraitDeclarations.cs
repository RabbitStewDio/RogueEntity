using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Players
{
    public static class PlayerTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder<TItemId> AsPlayer<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder)
            where TItemId : IEntityKey
        {
            var trait = new PlayerTrait<TItemId>();
            builder.Declaration.WithTrait(trait);
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TItemId> AsSpawnLocation<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder)
            where TItemId : IEntityKey
        {
            var trait = new PlayerSpawnLocationTrait<TItemId>();
            builder.Declaration.WithTrait(trait);
            return builder;
        }

        
    }
}
