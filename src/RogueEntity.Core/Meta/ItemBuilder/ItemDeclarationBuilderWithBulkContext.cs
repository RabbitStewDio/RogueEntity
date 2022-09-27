using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Services;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemBuilder
{
    public readonly struct ItemDeclarationBuilderWithBulkContext<TItemId>
        where TItemId : struct, IEntityKey
    {
        readonly IServiceResolver serviceResolver;

        public ItemDeclarationBuilderWithBulkContext(IServiceResolver serviceResolver)
        {
            this.serviceResolver = serviceResolver;
        }

        public BulkItemDeclarationBuilder<TItemId> Define(ItemDeclarationInfo id)
        {
            return new BulkItemDeclarationBuilder<TItemId>(serviceResolver, new BulkItemDeclaration<TItemId>(id.Id, id.Tag));
        }

        public BulkItemDeclarationBuilder<TItemId> Define(ItemDeclarationId id, WorldEntityTag tag = default)
        {
            return new BulkItemDeclarationBuilder<TItemId>(serviceResolver, new BulkItemDeclaration<TItemId>(id, tag));
        }
    }
}
