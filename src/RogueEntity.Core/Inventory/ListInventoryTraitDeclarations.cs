using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Inventory
{
    public static class ListInventoryTraitDeclarations
    {
        public readonly struct InventoryDeclarationBuilder<TGameContext, TContainerEntityId>
            where TContainerEntityId : IEntityKey
        {
            readonly ReferenceItemDeclarationBuilder<TGameContext, TContainerEntityId> builder;
            readonly Weight defaultCarryWeight;

            public InventoryDeclarationBuilder(ReferenceItemDeclarationBuilder<TGameContext, TContainerEntityId> builder, Weight defaultCarryWeight)
            {
                this.builder = builder;
                this.defaultCarryWeight = defaultCarryWeight;
            }

            public ReferenceItemDeclarationBuilder<TGameContext, TContainerEntityId> Of<TContentEntityId>()
                where TContentEntityId : IEntityKey
            {
                var resolver = builder.ServiceResolver.Resolve<IItemResolver<TGameContext, TContentEntityId>>();
                var meta = builder.ServiceResolver.Resolve<IBulkDataStorageMetaData<TContentEntityId>>();
                var trait = new ListInventoryTrait<TGameContext, TContainerEntityId, TContentEntityId>(meta, resolver, defaultCarryWeight);
                builder.Declaration.WithTrait(trait);
                return builder;
            }
        }

        public static InventoryDeclarationBuilder<TGameContext, TContainerEntityId>
            WithInventory<TGameContext, TContainerEntityId>(this ReferenceItemDeclarationBuilder<TGameContext, TContainerEntityId> builder,
                                                                              Weight defaultCarryWeight = default)
            where TContainerEntityId : IEntityKey
        {
            return new InventoryDeclarationBuilder<TGameContext, TContainerEntityId>(builder, defaultCarryWeight);
        }
    }
}