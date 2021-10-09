using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Players
{
    public static class PlayerTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder<TItemId> AsPlayer<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new PlayerTrait<TItemId>());
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TItemId> AsAvatar<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder, bool observePlayerSelf = true)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new PlayerObserverTrait<TItemId>());
            return builder;
        }
    }
}
