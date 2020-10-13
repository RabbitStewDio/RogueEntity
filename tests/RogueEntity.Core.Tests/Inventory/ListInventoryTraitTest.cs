using System.Collections.Generic;
using EnTTSharp.Annotations;
using EnTTSharp.Entities;
using EnTTSharp.Serialization;
using NUnit.Framework;
using RogueEntity.Core.Infrastructure.Serialization;
using RogueEntity.Core.Inventory;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Tests.Meta.Items;

namespace RogueEntity.Core.Tests.Inventory
{
    [TestFixture]
    public class ListInventoryTraitTest :
        ItemComponentTraitTestBase<InventoryTestContext, ActorReference, IInventory<InventoryTestContext, ItemReference>, ListInventoryTrait<InventoryTestContext, ActorReference, ItemReference>>
    {
        readonly ItemDeclarationId StackedBulkItem = "inventory.bulk.stacked";
        readonly ItemDeclarationId ReferenceItem = "inventory.reference";

        protected override IItemComponentTestDataFactory<IInventory<InventoryTestContext, ItemReference>> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            return new ItemComponentTestDataFactory<IInventory<InventoryTestContext, ItemReference>>(
                       CreateInventory(relations.DefaultEntityId, Weight.Empty, Weight.Empty),
                       CreateInventory(relations.DefaultEntityId, Weight.Empty, Weight.Empty, Context.ItemResolver.Instantiate(Context, StackedBulkItem)),
                       CreateInventory(relations.DefaultEntityId, Weight.Empty, Weight.Empty, Context.ItemResolver.Instantiate(Context, ReferenceItem))
                   )
                   .WithRemoveProhibited()
                   .WithInvalidResult(CreateInventory(relations.AlternativeEntityId1, Weight.Empty, Weight.Empty));
        }

        ListInventory<InventoryTestContext, ActorReference, ItemReference> CreateInventory(ActorReference owner,
                                                                                           Weight totalWeight,
                                                                                           Weight availableWeight,
                                                                                           params ItemReference[] items)
        {
            return new ListInventory<InventoryTestContext, ActorReference, ItemReference>(Context.ItemResolver,
                                                                                          new ListInventoryData<ActorReference, ItemReference>(
                                                                                              owner, totalWeight, availableWeight, new List<ItemReference>(items)));
        }

        protected override InventoryTestContext CreateContext()
        {
            var context = new InventoryTestContext();
            context.ItemEntities.RegisterNonConstructable<ItemDeclarationHolder<InventoryTestContext, ItemReference>>();
            context.ItemEntities.RegisterNonConstructable<ContainerEntityMarker<ActorReference>>();
            context.ActorEntities.RegisterNonConstructable<ListInventoryData<ActorReference, ItemReference>>();

            context.ItemRegistry.Register(new BulkItemDeclaration<InventoryTestContext, ItemReference>(StackedBulkItem));
            context.ItemRegistry.Register(new ReferenceItemDeclaration<InventoryTestContext, ItemReference>(ReferenceItem).WithTrait(new ContainerEntityMarkerTrait<InventoryTestContext, ItemReference, ActorReference>()));
            return context;
        }

        protected override EntityComponentRegistration PerformEntityComponentRegistration(EntityRegistrationScanner scn)
        {
            if (!scn.TryRegisterComponent<ListInventoryData<ActorReference, ItemReference>>(out var registration))
            {
                Assert.Fail("Unable to register component type.");
            }

            return registration;
        }

        protected override EntityRegistry<ActorReference> EntityRegistry => Context.ActorEntities;
        protected override ItemRegistry<InventoryTestContext, ActorReference> ItemRegistry => Context.ActorRegistry;
        protected override IBulkDataStorageMetaData<ActorReference> ItemIdMetaData => new ActorReferenceMetaData();

        protected override ListInventoryTrait<InventoryTestContext, ActorReference, ItemReference> CreateTrait()
        {
            return new ListInventoryTrait<InventoryTestContext, ActorReference, ItemReference>(Context.ItemResolver);
        }

        protected override void CustomizeBinarySerializationContext(EntityKeyMapper<ActorReference> mapper, 
                                                                    BinarySerializationContext bs)
        {
            ItemReference IdentityMapper(EntityKeyData data) => ItemReference.FromReferencedItem(data.Age, data.Key);
            
            var itemReferenceMetaData = new ItemReferenceMetaData();
            var bulkIdSerializationMapper = new BulkItemIdSerializationMapper<ItemReference>(itemReferenceMetaData.BulkDataFactory, Context.ItemRegistry, Context.ItemRegistry);
            bs.Register(new ItemDeclarationHolderMessagePackFormatter<InventoryTestContext, ItemReference>(Context));
            bs.Register(new BulkKeyMessagePackFormatter<ItemReference>(itemReferenceMetaData, IdentityMapper, bulkIdSerializationMapper.TryMap));
        }

        protected override void CustomizeXmlSerializationContext(EntityKeyMapper<ActorReference> mapper, XmlSerializationContext bs)
        {
            ItemReference IdentityMapper(EntityKeyData data) => ItemReference.FromReferencedItem(data.Age, data.Key);
            
            var itemReferenceMetaData = new ItemReferenceMetaData();
            var bulkIdSerializationMapper = new BulkItemIdSerializationMapper<ItemReference>(itemReferenceMetaData.BulkDataFactory, Context.ItemRegistry, Context.ItemRegistry);
            bs.Register(new ItemDeclarationHolderSurrogateProvider<InventoryTestContext, ItemReference>(Context));
            bs.Register(new BulkKeySurrogateProvider<ItemReference>(itemReferenceMetaData, IdentityMapper, bulkIdSerializationMapper.TryMap));
        }
    }
}