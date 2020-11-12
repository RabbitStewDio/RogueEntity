using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Sensing.Discovery
{
    public static class SenseDiscoveryTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithDiscoveryMap<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            builder.Declaration.WithTrait(new DiscoveryMapTrait<TGameContext, TItemId>());
            return builder;
        }        
    }
}