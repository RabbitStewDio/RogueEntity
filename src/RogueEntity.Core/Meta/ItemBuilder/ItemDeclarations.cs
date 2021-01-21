using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Services;
using RogueEntity.Core.Meta.Items;

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
            where TItemId : IEntityKey
        {
            return new ItemDeclarationBuilderWithReferenceContext<TItemId>(serviceResolver);
        }

        public static ItemDeclarationBuilderWithBulkContext<TItemId> CreateBulkEntityBuilder<TItemId>(this IModuleContentContext<TItemId> ctx,
                                                                                                      IServiceResolver serviceResolver)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return new ItemDeclarationBuilderWithBulkContext<TItemId>(serviceResolver);
        }

        public static ReferenceItemDeclarationBuilder<TItemId> AsBuilder<TItemId>(this ReferenceItemDeclaration<TItemId> item, IServiceResolver serviceResolver)
            where TItemId : IEntityKey
        {
            return new ReferenceItemDeclarationBuilder<TItemId>(serviceResolver, item);
        }
        
        public static BulkItemDeclarationBuilder<TItemId> AsBuilder<TItemId>(this BulkItemDeclaration<TItemId> item, IServiceResolver serviceResolver)
            where TItemId : IEntityKey
        {
            return new BulkItemDeclarationBuilder<TItemId>(serviceResolver, item);
        }
    }

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

    public readonly struct ItemDeclarationBuilderWithReferenceContext<TItemId>
        where TItemId : IEntityKey
    {
        readonly IServiceResolver serviceResolver;

        public ItemDeclarationBuilderWithReferenceContext(IServiceResolver serviceResolver)
        {
            this.serviceResolver = serviceResolver;
        }

        public ReferenceItemDeclarationBuilder<TItemId> Define(ItemDeclarationId id, string tag = null)
        {
            return new ReferenceItemDeclarationBuilder<TItemId>(serviceResolver, new ReferenceItemDeclaration<TItemId>(id, tag));
        }
    }

    public readonly struct ItemDeclarationBuilderWithBulkContext<TItemId>
        where TItemId : IEntityKey
    {
        readonly IServiceResolver serviceResolver;

        public ItemDeclarationBuilderWithBulkContext(IServiceResolver serviceResolver)
        {
            this.serviceResolver = serviceResolver;
        }

        public BulkItemDeclarationBuilder<TItemId> Define(ItemDeclarationId id, string tag = null)
        {
            return new BulkItemDeclarationBuilder<TItemId>(serviceResolver, new BulkItemDeclaration<TItemId>(id, tag));
        }
    }

    public readonly struct ReferenceItemDeclarationBuilder<TItemId>
        where TItemId : IEntityKey
    {
        public readonly IServiceResolver ServiceResolver;
        public readonly ReferenceItemDeclaration<TItemId> Declaration;

        public ReferenceItemDeclarationBuilder(IServiceResolver serviceResolver,
                                               ReferenceItemDeclaration<TItemId> declaration)
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
        where TItemId : IEntityKey
    {
        public readonly IServiceResolver ServiceResolver;
        public readonly BulkItemDeclaration<TItemId> Declaration;

        public BulkItemDeclarationBuilder(IServiceResolver serviceResolver,
                                          BulkItemDeclaration<TItemId> declaration)
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
