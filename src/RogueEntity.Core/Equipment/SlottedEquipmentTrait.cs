using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Equipment
{
    public class SlottedEquipmentTrait<TGameContext, TActorId, TItemId> : IReferenceItemTrait<TGameContext, TActorId>,
                                                                          IItemComponentTrait<TGameContext, TActorId, ISlottedEquipment<TGameContext, TItemId>>,
                                                                          IItemComponentInformationTrait<TGameContext, TActorId, MaximumCarryWeight>
        where TItemId : IBulkDataStorageKey<TItemId>
        where TActorId : IBulkDataStorageKey<TActorId>
        where TGameContext: IItemContext<TGameContext, TActorId>
    {
        readonly Weight maximumCarryWeight;
        readonly ReadOnlyListWrapper<EquipmentSlot> availableSlots;
        readonly IItemResolver<TGameContext, TItemId> itemResolver;

        public SlottedEquipmentTrait(IItemResolver<TGameContext, TItemId> itemResolver,
                                     Weight maximumCarryWeight,
                                     params EquipmentSlot[] availableSlots) : this(itemResolver, maximumCarryWeight, (IEnumerable<EquipmentSlot>)availableSlots)
        {
        }
        
        public SlottedEquipmentTrait(IItemResolver<TGameContext, TItemId> itemResolver,
                                     Weight maximumCarryWeight,
                                     IEnumerable<EquipmentSlot> availableSlots)
        {
            this.itemResolver = itemResolver;
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

        public string Id => "Core.Actor.Equipment";

        public int Priority => 300;

        public IReferenceItemTrait<TGameContext, TActorId> CreateInstance()
        {
            return this;
        }

        public void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            v.AssignOrReplace(k, SlottedEquipmentData<TItemId>.Create());
        }

        public void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out MaximumCarryWeight t)
        {
            t = new MaximumCarryWeight(maximumCarryWeight);
            return true;
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out ISlottedEquipment<TGameContext, TItemId> t)
        {
            if (v.GetComponent(k, out SlottedEquipmentData<TItemId> data))
            {
                if (context.ItemResolver.TryQueryData(k, context, out MaximumCarryWeight weight))
                {
                    t = new SlottedEquipment<TGameContext, TActorId, TItemId>(itemResolver, availableSlots, data, weight.CarryWeight).RefreshWeight(context);
                }
                else
                {
                    t = new SlottedEquipment<TGameContext, TActorId, TItemId>(itemResolver, availableSlots, data, maximumCarryWeight).RefreshWeight(context);
                }
                
                return true;
            }

            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, in ISlottedEquipment<TGameContext, TItemId> t, out TActorId changedK)
        {
            if (t is SlottedEquipment<TGameContext, TActorId, TItemId> st)
            {
                ref var data = ref st.Data;
                v.AssignOrReplace(k, in data);
                changedK = k;
                return true;
            }

            changedK = k;
            return false;
        }

        public bool TryRemove(IEntityViewControl<TActorId> entityRegistry, TGameContext context, TActorId k, out TActorId changedItem)
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

        /* TODO Move to a status effects module.
        public override void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            if (!itemResolver.TryQueryData(k, context, out ActiveActorStatusEffects<TGameContext> statusEffects))
            {
                return;
            }

            if (context.ActorResolver.TryQueryData(actorInfo.Reference, context, out SlottedEquipment<TGameContext> equipment))
            {
                foreach (var e in equipment)
                {
                    if (!context.ItemResolver.TryQueryData(e.Reference, context, out EquipmentStatusEffect<TGameContext> effects))
                    {
                        continue;
                    }

                    foreach (var effect in effects)
                    {
                        statusEffects.Apply(context, effect, e.Reference);
                    }
                }
            }
        }
        */
    }
}