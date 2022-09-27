using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Equipment
{
    public class SlottedEquipmentTrait<TActorId, TItemId> : IReferenceItemTrait<TActorId>,
                                                            IItemComponentTrait<TActorId, ISlottedEquipment<TItemId>>,
                                                            IItemComponentInformationTrait<TActorId, MaximumCarryWeight>
        where TItemId : struct, IEntityKey
        where TActorId : struct, IEntityKey
    {
        readonly Weight maximumCarryWeight;
        readonly ReadOnlyListWrapper<EquipmentSlot> availableSlots;
        readonly IBulkDataStorageMetaData<TItemId> itemIdMetaData;
        readonly IItemResolver<TItemId> itemResolver;
        readonly IItemResolver<TActorId> actorResolver;

        public SlottedEquipmentTrait(IItemResolver<TActorId> actorResolver,
                                     IItemResolver<TItemId> itemResolver,
                                     IBulkDataStorageMetaData<TItemId> itemIdMetaData,
                                     Weight maximumCarryWeight,
                                     params EquipmentSlot[] availableSlots) : this(actorResolver, itemResolver, itemIdMetaData, maximumCarryWeight, (IEnumerable<EquipmentSlot>)availableSlots)
        { }

        public SlottedEquipmentTrait(IItemResolver<TActorId> actorResolver,
                                     IItemResolver<TItemId> itemResolver,
                                     IBulkDataStorageMetaData<TItemId> itemIdMetaData,
                                     Weight maximumCarryWeight,
                                     IEnumerable<EquipmentSlot> availableSlots)
        {
            this.itemIdMetaData = itemIdMetaData ?? throw new ArgumentNullException(nameof(itemIdMetaData));
            this.actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
            var equipmentSlots = new List<EquipmentSlot>();
            foreach (var a in availableSlots)
            {
                if (!equipmentSlots.Contains(a))
                {
                    equipmentSlots.Add(a);
                }
            }

            equipmentSlots.Sort(EquipmentSlot.OrderComparer.Compare);
            this.availableSlots = equipmentSlots;
            this.maximumCarryWeight = maximumCarryWeight;
        }

        public ItemTraitId Id => "Core.Actor.Equipment";

        public int Priority => 300;

        public IReferenceItemTrait<TActorId> CreateInstance()
        {
            return this;
        }

        public void Initialize(IEntityViewControl<TActorId> v, TActorId k, IItemDeclaration item)
        {
            v.AssignOrReplace(k, SlottedEquipmentData<TItemId>.Create());
        }

        public void Apply(IEntityViewControl<TActorId> v,  TActorId k, IItemDeclaration item)
        { }

        public bool TryQuery(IEntityViewControl<TActorId> v, TActorId k, out MaximumCarryWeight t)
        {
            t = new MaximumCarryWeight(maximumCarryWeight);
            return true;
        }

        public bool TryQuery(IEntityViewControl<TActorId> v,  TActorId k, [MaybeNullWhen(false)] out ISlottedEquipment<TItemId> t)
        {
            if (v.GetComponent(k, out SlottedEquipmentData<TItemId> data))
            {
                if (actorResolver.TryQueryData(k,  out MaximumCarryWeight weight))
                {
                    t = new SlottedEquipment<TActorId, TItemId>(itemIdMetaData, itemResolver, availableSlots, data, weight.CarryWeight).RefreshWeight();
                }
                else
                {
                    t = new SlottedEquipment<TActorId, TItemId>(itemIdMetaData, itemResolver, availableSlots, data, maximumCarryWeight).RefreshWeight();
                }

                return true;
            }

            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TActorId> v, TActorId k, in ISlottedEquipment<TItemId> t, out TActorId changedK)
        {
            if (t is SlottedEquipment<TActorId, TItemId> st)
            {
                ref var data = ref st.Data;
                v.AssignOrReplace(k, in data);
                changedK = k;
                return true;
            }

            changedK = k;
            return false;
        }

        public bool TryRemove(IEntityViewControl<TActorId> entityRegistry, TActorId k, out TActorId changedItem)
        {
            changedItem = k;
            return false;
        }

        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return EquipmentModule.EquipmentContainerRole.Instantiate<TActorId>();
            yield return EquipmentModule.EquipmentContainedItemRole.Instantiate<TItemId>();
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            yield return EquipmentModule.CanEquipRelation.Instantiate<TActorId, TItemId>();
        }
    }
}
