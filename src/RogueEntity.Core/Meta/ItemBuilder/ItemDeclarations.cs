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
            return new ItemDeclarationBuilderWithReferenceContext<TItemId>(ctx, serviceResolver);
        }

        public static ItemDeclarationBuilderWithBulkContext<TItemId> CreateBulkEntityBuilder<TItemId>(this IModuleContentContext<TItemId> ctx,
                                                                                                      IServiceResolver serviceResolver)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return new ItemDeclarationBuilderWithBulkContext<TItemId>(ctx, serviceResolver);
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
            return new ItemDeclarationBuilderWithReferenceContext<TItemId>(mod.DeclareContentContext<TItemId>(), serviceResolver);
        }

        public ItemDeclarationBuilderWithBulkContext<TItemId> ForBulkEntity<TItemId>()
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return new ItemDeclarationBuilderWithBulkContext<TItemId>(mod.DeclareContentContext<TItemId>(), serviceResolver);
        }
    }

    public readonly struct ItemDeclarationBuilderWithReferenceContext<TItemId>
        where TItemId : IEntityKey
    {
        readonly IModuleContentContext<TItemId> entityContext;
        readonly IServiceResolver serviceResolver;

        public ItemDeclarationBuilderWithReferenceContext(IModuleContentContext<TItemId> entityContext,
                                                          IServiceResolver serviceResolver)
        {
            this.entityContext = entityContext;
            this.serviceResolver = serviceResolver;
        }

        public ReferenceItemDeclarationBuilder<TItemId> Define(ItemDeclarationId id, string tag = null)
        {
            return new ReferenceItemDeclarationBuilder<TItemId>(entityContext, serviceResolver, new ReferenceItemDeclaration<TItemId>(id, tag));
        }
    }

    public readonly struct ItemDeclarationBuilderWithBulkContext<TItemId>
        where TItemId : IEntityKey
    {
        readonly IModuleContentContext<TItemId> entityContext;
        readonly IServiceResolver serviceResolver;

        public ItemDeclarationBuilderWithBulkContext(IModuleContentContext<TItemId> entityContext,
                                                     IServiceResolver serviceResolver)
        {
            this.entityContext = entityContext;
            this.serviceResolver = serviceResolver;
        }

        public BulkItemDeclarationBuilder<TItemId> Define(ItemDeclarationId id, string tag = null)
        {
            return new BulkItemDeclarationBuilder<TItemId>(entityContext, serviceResolver, new BulkItemDeclaration<TItemId>(id, tag));
        }

        public BulkItemDeclarationBuilder<TItemId> RedefineAs(ItemDeclarationId id, ItemDeclarationId copyId, string tag = null)
        {
            var declaration = new BulkItemDeclaration<TItemId>(copyId, tag);
            if (entityContext.TryGetDefinedBulkItem(id, out var bi))
            {
                foreach (var trait in bi.QueryAll<IBulkItemTrait<TItemId>>())
                {
                    declaration.WithTrait(trait);
                }
            }

            return new BulkItemDeclarationBuilder<TItemId>(entityContext, serviceResolver, declaration);
        }
    }

    public readonly struct ReferenceItemDeclarationBuilder<TItemId>
        where TItemId : IEntityKey
    {
        public readonly IModuleContentContext<TItemId> EntityContext;
        public readonly IServiceResolver ServiceResolver;
        public readonly ReferenceItemDeclaration<TItemId> Declaration;

        public ReferenceItemDeclarationBuilder(IModuleContentContext<TItemId> entityContext,
                                               IServiceResolver serviceResolver,
                                               ReferenceItemDeclaration<TItemId> declaration)
        {
            this.EntityContext = entityContext;
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
        public readonly IModuleContentContext<TItemId> EntityContext;
        public readonly IServiceResolver ServiceResolver;
        public readonly BulkItemDeclaration<TItemId> Declaration;

        public BulkItemDeclarationBuilder(IModuleContentContext<TItemId> entityContext,
                                          IServiceResolver serviceResolver,
                                          BulkItemDeclaration<TItemId> declaration)
        {
            this.EntityContext = entityContext;
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
