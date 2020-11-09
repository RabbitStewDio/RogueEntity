using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemBuilder
{
    public static class ItemDeclarations
    {
        public static ItemDeclarationBuilder<TGameContext> Create<TGameContext>(this IModuleInitializer<TGameContext> mod,
                                                                                IServiceResolver serviceResolver)
        {
            return new ItemDeclarationBuilder<TGameContext>(mod, serviceResolver);
        }

        public static ItemDeclarationBuilderWithReferenceContext<TGameContext, TItemId> CreateReferenceEntityBuilder<TGameContext, TItemId>(this IModuleEntityContext<TGameContext, TItemId> ctx,
                                                                                                                                            IServiceResolver serviceResolver)
            where TItemId : IEntityKey
        {
            return new ItemDeclarationBuilderWithReferenceContext<TGameContext, TItemId>(ctx, serviceResolver);
        }

        public static ItemDeclarationBuilderWithBulkContext<TGameContext, TItemId> CreateBulkEntityBuilder<TGameContext, TItemId>(this IModuleEntityContext<TGameContext, TItemId> ctx,
                                                                                                                                  IServiceResolver serviceResolver)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return new ItemDeclarationBuilderWithBulkContext<TGameContext, TItemId>(ctx, serviceResolver);
        }
    }

    public readonly struct ItemDeclarationBuilder<TGameContext>
    {
        readonly IModuleInitializer<TGameContext> mod;
        readonly IServiceResolver serviceResolver;

        public ItemDeclarationBuilder(IModuleInitializer<TGameContext> mod, IServiceResolver serviceResolver)
        {
            this.mod = mod;
            this.serviceResolver = serviceResolver;
        }

        public ItemDeclarationBuilderWithReferenceContext<TGameContext, TItemId> ForEntity<TItemId>()
            where TItemId : IEntityKey
        {
            return new ItemDeclarationBuilderWithReferenceContext<TGameContext, TItemId>(mod.DeclareEntityContext<TItemId>(), serviceResolver);
        }

        public ItemDeclarationBuilderWithBulkContext<TGameContext, TItemId> ForBulkEntity<TItemId>()
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return new ItemDeclarationBuilderWithBulkContext<TGameContext, TItemId>(mod.DeclareEntityContext<TItemId>(), serviceResolver);
        }
    }

    public readonly struct ItemDeclarationBuilderWithReferenceContext<TGameContext, TItemId>
        where TItemId : IEntityKey
    {
        readonly IModuleEntityContext<TGameContext, TItemId> entityContext;
        readonly IServiceResolver serviceResolver;

        public ItemDeclarationBuilderWithReferenceContext(IModuleEntityContext<TGameContext, TItemId> entityContext,
                                                          IServiceResolver serviceResolver)
        {
            this.entityContext = entityContext;
            this.serviceResolver = serviceResolver;
        }

        public ReferenceItemDeclarationBuilder<TGameContext, TItemId> Define(ItemDeclarationId id, string tag = null)
        {
            return new ReferenceItemDeclarationBuilder<TGameContext, TItemId>(entityContext, serviceResolver, new ReferenceItemDeclaration<TGameContext, TItemId>(id, tag));
        }
    }

    public readonly struct ItemDeclarationBuilderWithBulkContext<TGameContext, TItemId>
        where TItemId : IEntityKey
    {
        readonly IModuleEntityContext<TGameContext, TItemId> entityContext;
        readonly IServiceResolver serviceResolver;

        public ItemDeclarationBuilderWithBulkContext(IModuleEntityContext<TGameContext, TItemId> entityContext,
                                                     IServiceResolver serviceResolver)
        {
            this.entityContext = entityContext;
            this.serviceResolver = serviceResolver;
        }

        public BulkItemDeclarationBuilder<TGameContext, TItemId> Define(ItemDeclarationId id, string tag = null)
        {
            return new BulkItemDeclarationBuilder<TGameContext, TItemId>(entityContext, serviceResolver, new BulkItemDeclaration<TGameContext, TItemId>(id, tag));
        }

        public BulkItemDeclarationBuilder<TGameContext, TItemId> RedefineAs(ItemDeclarationId id, ItemDeclarationId copyId, string tag = null)
        {
            var declaration = new BulkItemDeclaration<TGameContext, TItemId>(copyId, tag);
            if (entityContext.TryGetDefinedBulkItem(id, out var bi))
            {
                foreach (var trait in bi.QueryAll<IBulkItemTrait<TGameContext, TItemId>>())
                {
                    declaration.WithTrait(trait);
                }
            }
            return new BulkItemDeclarationBuilder<TGameContext, TItemId>(entityContext, serviceResolver, declaration);
        }
    }

    public readonly struct ReferenceItemDeclarationBuilder<TGameContext, TItemId>
        where TItemId : IEntityKey
    {
        public readonly IModuleEntityContext<TGameContext, TItemId> EntityContext;
        public readonly IServiceResolver ServiceResolver;
        public readonly ReferenceItemDeclaration<TGameContext, TItemId> Declaration;

        public ReferenceItemDeclarationBuilder(IModuleEntityContext<TGameContext, TItemId> entityContext,
                                               IServiceResolver serviceResolver,
                                               ReferenceItemDeclaration<TGameContext, TItemId> declaration)
        {
            this.EntityContext = entityContext;
            this.ServiceResolver = serviceResolver;
            this.Declaration = declaration;
        }

        public ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithTrait(IReferenceItemTrait<TGameContext, TItemId> trait)
        {
            Declaration.WithTrait(trait);
            return this;
        }
    }

    public readonly struct BulkItemDeclarationBuilder<TGameContext, TItemId>
        where TItemId : IEntityKey
    {
        public readonly IModuleEntityContext<TGameContext, TItemId> EntityContext;
        public readonly IServiceResolver ServiceResolver;
        public readonly BulkItemDeclaration<TGameContext, TItemId> Declaration;

        public BulkItemDeclarationBuilder(IModuleEntityContext<TGameContext, TItemId> entityContext,
                                          IServiceResolver serviceResolver,
                                          BulkItemDeclaration<TGameContext, TItemId> declaration)
        {
            this.EntityContext = entityContext;
            this.ServiceResolver = serviceResolver;
            this.Declaration = declaration;
        }

        public BulkItemDeclarationBuilder<TGameContext, TItemId> WithTrait(IBulkItemTrait<TGameContext, TItemId> trait)
        {
            Declaration.WithTrait(trait);
            return this;
        }
    }
}