using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.MapLoading.PlayerSpawning
{
    public static class PlayerSpawnLocationDeclarations
    {
        public static ReferenceItemDeclarationBuilder<TItemId> AsSpawnLocation<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new PlayerSpawnLocationTrait<TItemId>());
            return builder;
        }
        
    }
}
