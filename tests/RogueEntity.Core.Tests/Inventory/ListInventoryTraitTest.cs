using System.Collections.Generic;
using EnTTSharp.Annotations;
using EnTTSharp.Serialization;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
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

        public ListInventoryTraitTest() : base(new ActorReferenceMetaData())
        {
        }

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
            return new ListInventory<InventoryTestContext, ActorReference, ItemReference>(new ItemReferenceMetaData(), Context.ItemResolver,
                                                                                          new ListInventoryData<ActorReference, ItemReference>(
                                                                                              owner, totalWeight, availableWeight, new List<ItemReference>(items)));
        }

        protected override InventoryTestContext CreateContext()
        {
            EntityRegistry.RegisterNonConstructable<ListInventoryData<ActorReference, ItemReference>>();
            
            var context = new InventoryTestContext();
            context.ItemEntities.RegisterNonConstructable<ItemDeclarationHolder<InventoryTestContext, ItemReference>>();
            context.ItemEntities.RegisterNonConstructable<ContainerEntityMarker<ActorReference>>();

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

        protected override ListInventoryTrait<InventoryTestContext, ActorReference, ItemReference> CreateTrait()
        {
            return new ListInventoryTrait<InventoryTestContext, ActorReference, ItemReference>(new ItemReferenceMetaData(), Context.ItemResolver);
        }

        protected override void CustomizeBinarySerializationContext(EntityKeyMapper<ActorReference> mapper, 
                                                                    BinarySerializationContext bs)
        {
            ItemReference IdentityMapper(EntityKeyData data) => ItemReference.FromReferencedItem(data.Age, data.Key);
            
            var itemReferenceMetaData = new ItemReferenceMetaData();
            var bulkIdSerializationMapper = new BulkItemIdSerializationMapper<ItemReference>(itemReferenceMetaData, Context.ItemRegistry, Context.ItemRegistry);
            bs.Register(new ItemDeclarationHolderMessagePackFormatter<InventoryTestContext, ItemReference>(Context.ItemResolver));
            bs.Register(new BulkKeyMessagePackFormatter<ItemReference>(itemReferenceMetaData, IdentityMapper, bulkIdSerializationMapper.TryMap));
        }

        protected override void CustomizeXmlSerializationContext(EntityKeyMapper<ActorReference> mapper, XmlSerializationContext bs)
        {
            ItemReference IdentityMapper(EntityKeyData data) => ItemReference.FromReferencedItem(data.Age, data.Key);
            
            var itemReferenceMetaData = new ItemReferenceMetaData();
            var bulkIdSerializationMapper = new BulkItemIdSerializationMapper<ItemReference>(itemReferenceMetaData, Context.ItemRegistry, Context.ItemRegistry);
            bs.Register(new ItemDeclarationHolderSurrogateProvider<InventoryTestContext, ItemReference>(Context.ItemResolver));
            bs.Register(new BulkKeySurrogateProvider<ItemReference>(itemReferenceMetaData, IdentityMapper, bulkIdSerializationMapper.TryMap));
        }
    }
}