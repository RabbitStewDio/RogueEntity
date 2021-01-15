using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Sensing.Discovery
{
    public static class SenseDiscoveryTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder< TItemId> WithDiscoveryMap< TItemId>(this ReferenceItemDeclarationBuilder< TItemId> builder)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new DiscoveryMapTrait< TItemId>());
            return builder;
        }        
    }
}