using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Sensing.Discovery
{
    public static class SenseDiscoveryTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithDiscoveryMap<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new DiscoveryMapTrait<TGameContext, TItemId>());
            return builder;
        }        
    }
}