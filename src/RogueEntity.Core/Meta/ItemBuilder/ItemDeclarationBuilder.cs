using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Services;

namespace RogueEntity.Core.Meta.ItemBuilder
{
    public readonly struct ItemDeclarationBuilder
    {
        readonly IModuleInitializer mod;
        readonly IServiceResolver serviceResolver;

        public ItemDeclarationBuilder(IModuleInitializer mod, IServiceResolver serviceResolver)
        {
            this.mod = mod;
            this.serviceResolver = serviceResolver;
        }

        public ItemDeclarationBuilderWithReferenceContext<TItemId> ForEntity<TItemId>()
            where TItemId : IEntityKey
        {
            mod.DeclareContentContext<TItemId>();
            return new ItemDeclarationBuilderWithReferenceContext<TItemId>(serviceResolver);
        }

        public ItemDeclarationBuilderWithBulkContext<TItemId> ForBulkEntity<TItemId>()
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            mod.DeclareContentContext<TItemId>();
            return new ItemDeclarationBuilderWithBulkContext<TItemId>(serviceResolver);
        }
    }
}
