using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Services;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemBuilder
{
    public readonly struct ItemDeclarationBuilderWithReferenceContext<TItemId>
        where TItemId : struct, IEntityKey
    {
        readonly IServiceResolver serviceResolver;

        public ItemDeclarationBuilderWithReferenceContext(IServiceResolver serviceResolver)
        {
            this.serviceResolver = serviceResolver;
        }

        public ReferenceItemDeclarationBuilder<TItemId> Define(ItemDeclarationInfo id)
        {
            return new ReferenceItemDeclarationBuilder<TItemId>(serviceResolver, new ReferenceItemDeclaration<TItemId>(id.Id, id.Tag));
        }
        
        public ReferenceItemDeclarationBuilder<TItemId> Define(ItemDeclarationId id, WorldEntityTag tag = default)
        {
            return new ReferenceItemDeclarationBuilder<TItemId>(serviceResolver, new ReferenceItemDeclaration<TItemId>(id, tag));
        }
    }
}
