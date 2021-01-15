using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Inventory
{
    public static class ListInventoryTraitDeclarations
    {
        public readonly struct InventoryDeclarationBuilder< TContainerEntityId>
            where TContainerEntityId : IEntityKey
        {
            readonly ReferenceItemDeclarationBuilder< TContainerEntityId> builder;
            readonly Weight defaultCarryWeight;

            public InventoryDeclarationBuilder(ReferenceItemDeclarationBuilder< TContainerEntityId> builder, Weight defaultCarryWeight)
            {
                this.builder = builder;
                this.defaultCarryWeight = defaultCarryWeight;
            }

            public ReferenceItemDeclarationBuilder< TContainerEntityId> Of<TContentEntityId>()
                where TContentEntityId : IEntityKey
            {
                var resolver = builder.ServiceResolver.Resolve<IItemResolver< TContentEntityId>>();
                var meta = builder.ServiceResolver.Resolve<IBulkDataStorageMetaData<TContentEntityId>>();
                var trait = new ListInventoryTrait< TContainerEntityId, TContentEntityId>(meta, resolver, defaultCarryWeight);
                builder.Declaration.WithTrait(trait);
                return builder;
            }
        }

        public static InventoryDeclarationBuilder< TContainerEntityId>
            WithInventory< TContainerEntityId>(this ReferenceItemDeclarationBuilder< TContainerEntityId> builder,
                                                                              Weight defaultCarryWeight = default)
            where TContainerEntityId : IEntityKey
        {
            return new InventoryDeclarationBuilder< TContainerEntityId>(builder, defaultCarryWeight);
        }
    }
}