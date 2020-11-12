using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Meta
{
    public static class CoreEntityTraitDeclarations
    {
        public readonly struct ContentDeclarationBuilder<TGameContext, TItemId>
            where TItemId : IEntityKey
        {
            readonly ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder;

            public ContentDeclarationBuilder(ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder)
            {
                this.builder = builder;
            }

            public ReferenceItemDeclarationBuilder<TGameContext, TItemId> OfType<TContainerEntityType>()
            {
                var trait = new ContainerEntityMarkerTrait<TGameContext, TItemId, TContainerEntityType>();
                builder.Declaration.WithTrait(trait);
                return builder;
            }
        }

        public static ContentDeclarationBuilder<TGameContext, TItemId> CanPlaceIntoContainer<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder)
            where TItemId : IEntityKey
        {
            return new ContentDeclarationBuilder<TGameContext, TItemId>(builder);
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> AsPlayer<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder)
            where TItemId : IEntityKey
        {
            var trait = new PlayerTrait<TGameContext, TItemId>();
            builder.Declaration.WithTrait(trait);
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithWeight<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder, Weight weight)
            where TItemId : IEntityKey
        {
            var itemResolver = builder.ServiceResolver.Resolve<IItemResolver<TGameContext, TItemId>>();
            var trait = new WeightTrait<TGameContext, TItemId>(weight);
            builder.Declaration.WithTrait(trait);
            builder.Declaration.WithTrait(new WeightViewTrait<TGameContext, TItemId>(itemResolver));
            return builder;
        }

        public static BulkItemDeclarationBuilder<TGameContext, TItemId> WithWeight<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder, Weight weight)
            where TItemId : IEntityKey
        {
            var itemResolver = builder.ServiceResolver.Resolve<IItemResolver<TGameContext, TItemId>>();
            var trait = new WeightTrait<TGameContext, TItemId>(weight);
            builder.Declaration.WithTrait(trait);
            builder.Declaration.WithTrait(new WeightViewTrait<TGameContext, TItemId>(itemResolver));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithTemperature<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                    Temperature t)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new TemperatureTrait<TGameContext, TItemId>(t));
            return builder;
        }

        public static BulkItemDeclarationBuilder<TGameContext, TItemId> WithTemperature<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                               Temperature t)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new TemperatureTrait<TGameContext, TItemId>(t));
            return builder;
        }

        public static BulkItemDeclarationBuilder<TGameContext, TItemId> WithStackCount<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                              ushort stackSize)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return builder.WithStackCount(stackSize, stackSize);
        }

        public static BulkItemDeclarationBuilder<TGameContext, TItemId> WithStackCount<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                              ushort initialCount,
                                                                                                              ushort stackSize)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            builder.Declaration.WithTrait(new StackingTrait<TGameContext, TItemId>(initialCount, stackSize));
            return builder;
        }

        public static BulkItemDeclarationBuilder<TGameContext, TItemId> WithItemCharge<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                              ushort stackSize)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return builder.WithItemCharge(stackSize, stackSize);
        }

        public static BulkItemDeclarationBuilder<TGameContext, TItemId> WithItemCharge<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                              ushort initialCount,
                                                                                                              ushort stackSize)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            builder.Declaration.WithTrait(new ItemChargeTrait<TGameContext, TItemId>(initialCount, stackSize));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithItemCharge<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                   ushort stackSize)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return builder.WithItemCharge(stackSize, stackSize);
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithItemCharge<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                   ushort initialCount,
                                                                                                                   ushort stackSize)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            builder.Declaration.WithTrait(new ItemChargeTrait<TGameContext, TItemId>(initialCount, stackSize));
            return builder;
        }

        public static BulkItemDeclarationBuilder<TGameContext, TItemId> WithDurability<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                              ushort stackSize)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return builder.WithDurability(stackSize, stackSize);
        }

        public static BulkItemDeclarationBuilder<TGameContext, TItemId> WithDurability<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                              ushort initialCount,
                                                                                                              ushort stackSize)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            builder.Declaration.WithTrait(new DurabilityTrait<TGameContext, TItemId>(initialCount, stackSize));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithDurability<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                   ushort stackSize)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return builder.WithDurability(stackSize, stackSize);
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithDurability<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                   ushort initialCount,
                                                                                                                   ushort stackSize)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            builder.Declaration.WithTrait(new DurabilityTrait<TGameContext, TItemId>(initialCount, stackSize));
            return builder;
        }
    }
}