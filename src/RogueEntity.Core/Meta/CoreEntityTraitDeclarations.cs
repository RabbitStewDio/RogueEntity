using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Players;

namespace RogueEntity.Core.Meta
{
    public static class CoreEntityTraitDeclarations
    {
        public readonly struct ContentDeclarationBuilder<TItemId>
            where TItemId : IEntityKey
        {
            readonly ReferenceItemDeclarationBuilder<TItemId> builder;

            public ContentDeclarationBuilder(ReferenceItemDeclarationBuilder<TItemId> builder)
            {
                this.builder = builder;
            }

            public ReferenceItemDeclarationBuilder<TItemId> OfType<TContainerEntityType>()
            {
                var trait = new ContainerEntityMarkerTrait<TItemId, TContainerEntityType>();
                builder.Declaration.WithTrait(trait);
                return builder;
            }
        }

        public static ContentDeclarationBuilder<TItemId> CanPlaceIntoContainer<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder)
            where TItemId : IEntityKey
        {
            return new ContentDeclarationBuilder<TItemId>(builder);
        }

        public static ReferenceItemDeclarationBuilder<TItemId> AsPlayer<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder)
            where TItemId : IEntityKey
        {
            var trait = new PlayerTrait<TItemId>();
            builder.Declaration.WithTrait(trait);
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TItemId> WithWeight<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder, Weight weight)
            where TItemId : IEntityKey
        {
            var itemResolver = builder.ServiceResolver.Resolve<IItemResolver<TItemId>>();
            var trait = new WeightTrait<TItemId>(weight);
            builder.Declaration.WithTrait(trait);
            builder.Declaration.WithTrait(new WeightViewTrait<TItemId>(itemResolver));
            return builder;
        }

        public static BulkItemDeclarationBuilder<TItemId> WithWeight<TItemId>(this BulkItemDeclarationBuilder<TItemId> builder, Weight weight)
            where TItemId : IEntityKey
        {
            var itemResolver = builder.ServiceResolver.Resolve<IItemResolver<TItemId>>();
            var trait = new WeightTrait<TItemId>(weight);
            builder.Declaration.WithTrait(trait);
            builder.Declaration.WithTrait(new WeightViewTrait<TItemId>(itemResolver));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TItemId> WithTemperature<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder,
                                                                                        Temperature t)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new TemperatureTrait<TItemId>(t));
            return builder;
        }

        public static BulkItemDeclarationBuilder<TItemId> WithTemperature<TItemId>(this BulkItemDeclarationBuilder<TItemId> builder,
                                                                                   Temperature t)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new TemperatureTrait<TItemId>(t));
            return builder;
        }

        public static BulkItemDeclarationBuilder<TItemId> WithStackCount<TItemId>(this BulkItemDeclarationBuilder<TItemId> builder,
                                                                                  ushort stackSize)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return builder.WithStackCount(stackSize, stackSize);
        }

        public static BulkItemDeclarationBuilder<TItemId> WithStackCount<TItemId>(this BulkItemDeclarationBuilder<TItemId> builder,
                                                                                  ushort initialCount,
                                                                                  ushort stackSize)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            builder.Declaration.WithTrait(new StackingBulkTrait<TItemId>(initialCount, stackSize));
            return builder;
        }

        public static BulkItemDeclarationBuilder<TItemId> WithItemCharge<TItemId>(this BulkItemDeclarationBuilder<TItemId> builder,
                                                                                  ushort stackSize)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return builder.WithItemCharge(stackSize, stackSize);
        }

        public static BulkItemDeclarationBuilder<TItemId> WithItemCharge<TItemId>(this BulkItemDeclarationBuilder<TItemId> builder,
                                                                                  ushort initialCount,
                                                                                  ushort stackSize)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            builder.Declaration.WithTrait(new ItemChargeBulkTrait<TItemId>(initialCount, stackSize));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TItemId> WithItemCharge<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder,
                                                                                       ushort stackSize)
            where TItemId : IEntityKey
        {
            return builder.WithItemCharge(stackSize, stackSize);
        }

        public static ReferenceItemDeclarationBuilder<TItemId> WithItemCharge<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder,
                                                                                       ushort initialCount,
                                                                                       ushort stackSize)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new ItemChargeTrait<TItemId>(initialCount, stackSize));
            return builder;
        }

        public static BulkItemDeclarationBuilder<TItemId> WithDurability<TItemId>(this BulkItemDeclarationBuilder<TItemId> builder,
                                                                                  ushort stackSize)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return builder.WithDurability(stackSize, stackSize);
        }

        public static BulkItemDeclarationBuilder<TItemId> WithDurability<TItemId>(this BulkItemDeclarationBuilder<TItemId> builder,
                                                                                  ushort initialCount,
                                                                                  ushort stackSize)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            builder.Declaration.WithTrait(new DurabilityBulkTrait<TItemId>(initialCount, stackSize));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TItemId> WithDurability<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder,
                                                                                       ushort stackSize)
            where TItemId : IEntityKey
        {
            return builder.WithDurability(stackSize, stackSize);
        }

        public static ReferenceItemDeclarationBuilder<TItemId> WithDurability<TItemId>(this ReferenceItemDeclarationBuilder<TItemId> builder,
                                                                                       ushort initialCount,
                                                                                       ushort stackSize)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new DurabilityTrait<TItemId>(initialCount, stackSize));
            return builder;
        }
    }
}
