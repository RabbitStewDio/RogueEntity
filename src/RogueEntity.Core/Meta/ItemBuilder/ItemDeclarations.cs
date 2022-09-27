using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Services;

namespace RogueEntity.Core.Meta.ItemBuilder
{
    public static class ItemDeclarations
    {
        public static ItemDeclarationBuilder Create(this IModuleInitializer mod,
                                                    IServiceResolver serviceResolver)
        {
            return new ItemDeclarationBuilder(mod, serviceResolver);
        }

        public static ItemDeclarationBuilderWithReferenceContext<TItemId> CreateReferenceEntityBuilder<TItemId>(this IModuleContentContext<TItemId> ctx,
                                                                                                                IServiceResolver serviceResolver)
            where TItemId : struct, IEntityKey
        {
            return new ItemDeclarationBuilderWithReferenceContext<TItemId>(serviceResolver);
        }

        public static ItemDeclarationBuilderWithBulkContext<TItemId> CreateBulkEntityBuilder<TItemId>(this IModuleContentContext<TItemId> ctx,
                                                                                                      IServiceResolver serviceResolver)
            where TItemId : struct, IBulkDataStorageKey<TItemId>
        {
            return new ItemDeclarationBuilderWithBulkContext<TItemId>(serviceResolver);
        }

        public static ReferenceItemDeclarationBuilder<TItemId> AsBuilder<TItemId>(this IReferenceItemDeclaration<TItemId> item, IServiceResolver serviceResolver)
            where TItemId : struct, IEntityKey
        {
            return new ReferenceItemDeclarationBuilder<TItemId>(serviceResolver, item);
        }
        
        public static BulkItemDeclarationBuilder<TItemId> AsBuilder<TItemId>(this IBulkItemDeclaration<TItemId> item, IServiceResolver serviceResolver)
            where TItemId : struct, IEntityKey
        {
            return new BulkItemDeclarationBuilder<TItemId>(serviceResolver, item);
        }
    }

    public readonly struct ReferenceItemDeclarationBuilder<TItemId>
        where TItemId : struct, IEntityKey
    {
        public readonly IServiceResolver ServiceResolver;
        public readonly IReferenceItemDeclaration<TItemId> Declaration;

        public ReferenceItemDeclarationBuilder(IServiceResolver serviceResolver,
                                               IReferenceItemDeclaration<TItemId> declaration)
        {
            this.ServiceResolver = serviceResolver;
            this.Declaration = declaration;
        }

        public ReferenceItemDeclarationBuilder<TItemId> WithTrait(IReferenceItemTrait<TItemId> trait)
        {
            Declaration.WithTrait(trait);
            return this;
        }
    }

    public readonly struct BulkItemDeclarationBuilder<TItemId>
        where TItemId : struct, IEntityKey
    {
        public readonly IServiceResolver ServiceResolver;
        public readonly IBulkItemDeclaration<TItemId> Declaration;

        public BulkItemDeclarationBuilder(IServiceResolver serviceResolver,
                                          IBulkItemDeclaration<TItemId> declaration)
        {
            this.ServiceResolver = serviceResolver;
            this.Declaration = declaration;
        }

        public BulkItemDeclarationBuilder<TItemId> WithTrait(IBulkItemTrait<TItemId> trait)
        {
            Declaration.WithTrait(trait);
            return this;
        }
    }
}
