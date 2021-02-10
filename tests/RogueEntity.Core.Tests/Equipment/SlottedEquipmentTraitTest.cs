using System.Collections.Generic;
using EnTTSharp.Annotations;
using EnTTSharp.Entities;
using EnTTSharp.Serialization;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Equipment;
using RogueEntity.Core.Infrastructure.Serialization;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Tests.Meta.Items;

namespace RogueEntity.Core.Tests.Equipment
{
    [TestFixture]
    public class SlottedEquipmentTraitTest : ItemComponentTraitTestBase<ActorReference, ISlottedEquipment<ItemReference>, SlottedEquipmentTrait<ActorReference, ItemReference>>
    {
        readonly ItemDeclarationId StackedBulkItem = "equipment.bulk.stacked";
        readonly ItemDeclarationId ReferenceItem = "equipment.reference";

        readonly EquipmentSlotRegistry registry;
        readonly EquipmentSlot slotHead;
        readonly EquipmentSlot slotLeftHand;
        readonly EquipmentSlot slotRightHand;
        public EquipmentTestContext Context { get; private set; }

        public SlottedEquipmentTraitTest() : base(new ActorReferenceMetaData())
        {
            slotHead = new EquipmentSlot("slot.head", 0, "Head", "HEAD");
            slotLeftHand = new EquipmentSlot("slot.hand.left", 1, "Left Hand", "LHND");
            slotRightHand = new EquipmentSlot("slot.hand.right", 2, "Right Hand", "RHND");

            registry = new EquipmentSlotRegistry();
            registry.Register(slotHead);
            registry.Register(slotRightHand);
            registry.Register(slotLeftHand);
        }

        protected override void SetUpPrepare()
        {
            Context = new EquipmentTestContext();
            Context.ItemEntities.RegisterNonConstructable<ItemDeclarationHolder<ItemReference>>();
            Context.ItemEntities.RegisterNonConstructable<ContainerEntityMarker<ActorReference>>();
            EntityRegistry.RegisterNonConstructable<SlottedEquipmentData<ItemReference>>();

            Context.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(StackedBulkItem));
            Context.ItemRegistry.Register(
                new ReferenceItemDeclaration<ItemReference>(ReferenceItem).WithTrait(new ContainerEntityMarkerTrait<ItemReference, ActorReference>()));
        }

        protected override SlottedEquipmentTrait<ActorReference, ItemReference> CreateTrait()
        {
            var meta = new ItemReferenceMetaData();
            return new SlottedEquipmentTrait<ActorReference, ItemReference>(Context.ActorResolver, Context.ItemResolver, meta, Weight.Unlimited, slotHead, slotLeftHand,
                                                                            slotRightHand);
        }

        protected override EntityComponentRegistration PerformEntityComponentRegistration(EntityRegistrationScanner scn)
        {
            if (!scn.TryRegisterComponent<SlottedEquipmentData<ItemReference>>(out var registration))
            {
                Assert.Fail("Unable to register component type.");
            }

            return registration;
        }

        protected override void CustomizeXmlSerializationContext(IEntityKeyMapper mapper, XmlSerializationContext bs)
        {
            var itemReferenceMetaData = new ItemReferenceMetaData();
            var bulkIdSerializationMapper = new BulkItemIdSerializationMapper<ItemReference>(itemReferenceMetaData, Context.ItemRegistry, Context.ItemRegistry);
            bs.Register(new ItemDeclarationHolderSurrogateProvider<ItemReference>(Context.ItemResolver));
            bs.Register(new BulkKeySurrogateProvider<ItemReference>(itemReferenceMetaData, CreateEntityMapper(Context.ItemResolver.EntityMetaData), bulkIdSerializationMapper.TryMap));
            bs.Register(new EquipmentSlotSurrogateProvider(registry));
        }

        protected override void CustomizeBinarySerializationContext(IEntityKeyMapper mapper, BinarySerializationContext bs)
        {
            var itemReferenceMetaData = new ItemReferenceMetaData();
            var bulkIdSerializationMapper = new BulkItemIdSerializationMapper<ItemReference>(itemReferenceMetaData, Context.ItemRegistry, Context.ItemRegistry);
            bs.Register(new ItemDeclarationHolderMessagePackFormatter<ItemReference>(Context.ItemResolver));
            bs.Register(new BulkKeyMessagePackFormatter<ItemReference>(itemReferenceMetaData, CreateEntityMapper(Context.ItemResolver.EntityMetaData), bulkIdSerializationMapper.TryMap));
            bs.Register(new EquipmentSlotMessagePackFormatter(registry));
        }

        protected override IItemComponentTestDataFactory<ISlottedEquipment<ItemReference>> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            var slots = new ReadOnlyListWrapper<EquipmentSlot>(new List<EquipmentSlot>
            {
                slotHead, slotLeftHand, slotRightHand
            });

            var meta = new ItemReferenceMetaData();

            return new ItemComponentTestDataFactory<ISlottedEquipment<ItemReference>>(
                       new SlottedEquipment<ActorReference, ItemReference>(meta, Context.ItemResolver, slots,
                                                                           SlottedEquipmentData<ItemReference>.Create(), Weight.Unlimited),
                       new SlottedEquipment<ActorReference, ItemReference>(meta, Context.ItemResolver, slots,
                                                                           SlottedEquipmentData<ItemReference>
                                                                               .Create()
                                                                               .Equip(Context.ItemResolver.Instantiate(StackedBulkItem),
                                                                                      slotHead, new List<EquipmentSlot> {slotHead}),
                                                                           Weight.Unlimited),
                       new SlottedEquipment<ActorReference, ItemReference>(meta, Context.ItemResolver, slots,
                                                                           SlottedEquipmentData<ItemReference>
                                                                               .Create()
                                                                               .Equip(Context.ItemResolver.Instantiate(ReferenceItem),
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
            where TItemId : IEntityKey
        {
            if (data.TryEquip(item, primary, slots, out var result))
            {
                return result;
            }

            return data;
        }
    }
}
