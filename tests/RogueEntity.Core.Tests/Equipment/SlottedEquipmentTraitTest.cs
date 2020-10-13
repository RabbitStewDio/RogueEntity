using System.Collections.Generic;
using EnTTSharp.Annotations;
using EnTTSharp.Entities;
using EnTTSharp.Serialization;
using NUnit.Framework;
using RogueEntity.Core.Equipment;
using RogueEntity.Core.Infrastructure.Serialization;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Equipment
{
    [TestFixture]
    public class SlottedEquipmentTraitTest : ItemComponentTraitTestBase<
        EquipmentTestContext, ActorReference,
        ISlottedEquipment<EquipmentTestContext, ItemReference>,
        SlottedEquipmentTrait<EquipmentTestContext, ActorReference, ItemReference>>
    {
        readonly ItemDeclarationId StackedBulkItem = "equipment.bulk.stacked";
        readonly ItemDeclarationId ReferenceItem = "equipment.reference";

        readonly EquipmentSlotRegistry registry;
        readonly EquipmentSlot slotHead;
        readonly EquipmentSlot slotLeftHand;
        readonly EquipmentSlot slotRightHand;

        public SlottedEquipmentTraitTest()
        {
            slotHead = new EquipmentSlot("slot.head", 0, "Head", "HEAD");
            slotLeftHand = new EquipmentSlot("slot.hand.left", 1, "Left Hand", "LHND");
            slotRightHand = new EquipmentSlot("slot.hand.right", 2, "Right Hand", "RHND");

            registry = new EquipmentSlotRegistry();
            registry.Register(slotHead);
            registry.Register(slotRightHand);
            registry.Register(slotLeftHand);
        }

        protected override EntityRegistry<ActorReference> EntityRegistry => Context.ActorEntities;
        protected override ItemRegistry<EquipmentTestContext, ActorReference> ItemRegistry => Context.ActorRegistry;
        protected override IBulkDataStorageMetaData<ActorReference> ItemIdMetaData => new ActorReferenceMetaData();

        protected override EquipmentTestContext CreateContext()
        {
            var context = new EquipmentTestContext();
            context.ItemEntities.RegisterNonConstructable<ItemDeclarationHolder<EquipmentTestContext, ItemReference>>();
            context.ItemEntities.RegisterNonConstructable<ContainerEntityMarker<ActorReference>>();
            context.ActorEntities.RegisterNonConstructable<SlottedEquipmentData<ItemReference>>();

            context.ItemRegistry.Register(new BulkItemDeclaration<EquipmentTestContext, ItemReference>(StackedBulkItem));
            context.ItemRegistry.Register(
                new ReferenceItemDeclaration<EquipmentTestContext, ItemReference>(ReferenceItem).WithTrait(new ContainerEntityMarkerTrait<EquipmentTestContext, ItemReference, ActorReference>()));


            return context;
        }

        protected override SlottedEquipmentTrait<EquipmentTestContext, ActorReference, ItemReference> CreateTrait()
        {
            return new SlottedEquipmentTrait<EquipmentTestContext, ActorReference, ItemReference>(Context.ItemResolver, Weight.Unlimited, slotHead, slotLeftHand, slotRightHand);
        }

        protected override EntityComponentRegistration PerformEntityComponentRegistration(EntityRegistrationScanner scn)
        {
            if (!scn.TryRegisterComponent<SlottedEquipmentData<ItemReference>>(out var registration))
            {
                Assert.Fail("Unable to register component type.");
            }

            return registration;
        }

        protected override void CustomizeXmlSerializationContext(EntityKeyMapper<ActorReference> mapper, XmlSerializationContext bs)
        {
            ItemReference IdentityMapper(EntityKeyData data) => ItemReference.FromReferencedItem(data.Age, data.Key);

            var itemReferenceMetaData = new ItemReferenceMetaData();
            var bulkIdSerializationMapper = new BulkItemIdSerializationMapper<ItemReference>(itemReferenceMetaData.BulkDataFactory, Context.ItemRegistry, Context.ItemRegistry);
            bs.Register(new ItemDeclarationHolderSurrogateProvider<EquipmentTestContext, ItemReference>(Context));
            bs.Register(new BulkKeySurrogateProvider<ItemReference>(itemReferenceMetaData, IdentityMapper, bulkIdSerializationMapper.TryMap));
            bs.Register(new EquipmentSlotSurrogateProvider(registry));
        }

        protected override void CustomizeBinarySerializationContext(EntityKeyMapper<ActorReference> mapper, BinarySerializationContext bs)
        {
            ItemReference IdentityMapper(EntityKeyData data) => ItemReference.FromReferencedItem(data.Age, data.Key);

            var itemReferenceMetaData = new ItemReferenceMetaData();
            var bulkIdSerializationMapper = new BulkItemIdSerializationMapper<ItemReference>(itemReferenceMetaData.BulkDataFactory, Context.ItemRegistry, Context.ItemRegistry);
            bs.Register(new ItemDeclarationHolderMessagePackFormatter<EquipmentTestContext, ItemReference>(Context));
            bs.Register(new BulkKeyMessagePackFormatter<ItemReference>(itemReferenceMetaData, IdentityMapper, bulkIdSerializationMapper.TryMap));
            bs.Register(new EquipmentSlotMessagePackFormatter(registry));
        }

        protected override IItemComponentTestDataFactory<ISlottedEquipment<EquipmentTestContext, ItemReference>> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            var slots = new ReadOnlyListWrapper<EquipmentSlot>(new List<EquipmentSlot>
            {
                slotHead, slotLeftHand, slotRightHand
            });

            return new ItemComponentTestDataFactory<ISlottedEquipment<EquipmentTestContext, ItemReference>>(
                       new SlottedEquipment<EquipmentTestContext, ActorReference, ItemReference>(Context.ItemResolver, slots,
                                                                                                 SlottedEquipmentData<ItemReference>.Create(), Weight.Unlimited),
                       new SlottedEquipment<EquipmentTestContext, ActorReference, ItemReference>(Context.ItemResolver, slots,
                                                                                                 SlottedEquipmentData<ItemReference>
                                                                                                     .Create()
                                                                                                     .Equip(Context.ItemResolver.Instantiate(Context, StackedBulkItem),
                                                                                                            slotHead, new List<EquipmentSlot> {slotHead}),
                                                                                                 Weight.Unlimited),
                       new SlottedEquipment<EquipmentTestContext, ActorReference, ItemReference>(Context.ItemResolver, slots,
                                                                                                 SlottedEquipmentData<ItemReference>
                                                                                                     .Create()
                                                                                                     .Equip(Context.ItemResolver.Instantiate(Context, ReferenceItem),
                                                                                                            slotLeftHand,
                                                                                                            new List<EquipmentSlot> {slotLeftHand, slotRightHand}),
                                                                                                 Weight.Unlimited))
                   .WithoutInvalidResult()
                   .WithRemoveProhibited();
        }
    }

    public static class SlottedEquipmentExtensions
    {
        public static SlottedEquipmentData<TItemId> Equip<TItemId>(this SlottedEquipmentData<TItemId> data, TItemId item, EquipmentSlot primary, List<EquipmentSlot> slots)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            if (data.TryEquip(item, primary, slots, out var result))
            {
                return result;
            }

            return data;
        }
    }
}