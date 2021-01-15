using System.Collections.Generic;
using EnTTSharp.Annotations;
using EnTTSharp.Serialization;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Infrastructure.Serialization;
using RogueEntity.Core.Inventory;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Tests.Meta.Items;

namespace RogueEntity.Core.Tests.Inventory
{
    [TestFixture]
    public class ListInventoryTraitTest : ItemComponentTraitTestBase<ActorReference, IInventory<ItemReference>, ListInventoryTrait<ActorReference, ItemReference>>
    {
        readonly ItemDeclarationId StackedBulkItem = "inventory.bulk.stacked";
        readonly ItemDeclarationId ReferenceItem = "inventory.reference";

        public ListInventoryTraitTest() : base(new ActorReferenceMetaData())
        {
        }
        
        InventoryTestContext Context { get; set; }

        protected override void SetUpPrepare()
        {
            EntityRegistry.RegisterNonConstructable<ListInventoryData<ActorReference, ItemReference>>();
            
            Context = new InventoryTestContext();
            Context.ItemEntities.RegisterNonConstructable<ItemDeclarationHolder<ItemReference>>();
            Context.ItemEntities.RegisterNonConstructable<ContainerEntityMarker<ActorReference>>();

            Context.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(StackedBulkItem));
            Context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>(ReferenceItem).WithTrait(new ContainerEntityMarkerTrait<ItemReference, ActorReference>()));
        }

        protected override IItemComponentTestDataFactory<IInventory<ItemReference>> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            return new ItemComponentTestDataFactory<IInventory<ItemReference>>(
                       CreateInventory(relations.DefaultEntityId, Weight.Empty, Weight.Empty),
                       CreateInventory(relations.DefaultEntityId, Weight.Empty, Weight.Empty, Context.ItemResolver.Instantiate(StackedBulkItem)),
                       CreateInventory(relations.DefaultEntityId, Weight.Empty, Weight.Empty, Context.ItemResolver.Instantiate(ReferenceItem))
                   )
                   .WithRemoveProhibited()
                   .WithInvalidResult(CreateInventory(relations.AlternativeEntityId1, Weight.Empty, Weight.Empty));
        }

        ListInventory<ActorReference, ItemReference> CreateInventory(ActorReference owner,
                                                                                           Weight totalWeight,
                                                                                           Weight availableWeight,
                                                                                           params ItemReference[] items)
        {
            return new ListInventory<ActorReference, ItemReference>(new ItemReferenceMetaData(), Context.ItemResolver,
                                                                                          new ListInventoryData<ActorReference, ItemReference>(
                                                                                              owner, totalWeight, availableWeight, new List<ItemReference>(items)));
        }

        protected override EntityComponentRegistration PerformEntityComponentRegistration(EntityRegistrationScanner scn)
        {
            if (!scn.TryRegisterComponent<ListInventoryData<ActorReference, ItemReference>>(out var registration))
            {
                Assert.Fail("Unable to register component type.");
            }

            return registration;
        }

        protected override ListInventoryTrait<ActorReference, ItemReference> CreateTrait()
        {
            return new ListInventoryTrait<ActorReference, ItemReference>(new ItemReferenceMetaData(), Context.ItemResolver);
        }

        protected override void CustomizeBinarySerializationContext(EntityKeyMapper<ActorReference> mapper, 
                                                                    BinarySerializationContext bs)
        {
            ItemReference IdentityMapper(EntityKeyData data) => ItemReference.FromReferencedItem(data.Age, data.Key);
            
            var itemReferenceMetaData = new ItemReferenceMetaData();
            var bulkIdSerializationMapper = new BulkItemIdSerializationMapper<ItemReference>(itemReferenceMetaData, Context.ItemRegistry, Context.ItemRegistry);
            bs.Register(new ItemDeclarationHolderMessagePackFormatter<ItemReference>(Context.ItemResolver));
            bs.Register(new BulkKeyMessagePackFormatter<ItemReference>(itemReferenceMetaData, IdentityMapper, bulkIdSerializationMapper.TryMap));
        }

        protected override void CustomizeXmlSerializationContext(EntityKeyMapper<ActorReference> mapper, XmlSerializationContext bs)
        {
            ItemReference IdentityMapper(EntityKeyData data) => ItemReference.FromReferencedItem(data.Age, data.Key);
            
            var itemReferenceMetaData = new ItemReferenceMetaData();
            var bulkIdSerializationMapper = new BulkItemIdSerializationMapper<ItemReference>(itemReferenceMetaData, Context.ItemRegistry, Context.ItemRegistry);
            bs.Register(new ItemDeclarationHolderSurrogateProvider<ItemReference>(Context.ItemResolver));
            bs.Register(new BulkKeySurrogateProvider<ItemReference>(itemReferenceMetaData, IdentityMapper, bulkIdSerializationMapper.TryMap));
        }
    }
}