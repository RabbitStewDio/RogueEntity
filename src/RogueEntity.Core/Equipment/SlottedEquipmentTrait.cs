using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Equipment
{
    public class SlottedEquipmentTrait<TGameContext, TActorId, TItemId> : IReferenceItemTrait<TGameContext, TActorId>,
                                                                          IItemComponentTrait<TGameContext, TActorId, SlottedEquipment<TGameContext, TItemId>>
        where TItemId : IBulkDataStorageKey<TItemId>
        where TActorId : IEntityKey
    {
        readonly ReadOnlyListWrapper<EquipmentSlot> availableSlots;
        readonly IItemResolver<TGameContext, TItemId> itemResolver;

        public SlottedEquipmentTrait(IItemResolver<TGameContext, TItemId> itemResolver,
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
        }

        public string Id => "Core.Actor.Equipment";

        public int Priority => 300;

        public void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            v.AssignOrReplace(k, new SlottedEquipmentData<TItemId>());
        }

        public void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out SlottedEquipment<TGameContext, TItemId> t)
        {
            if (v.GetComponent(k, out SlottedEquipmentData<TItemId> data))
            {
                t = new SlottedEquipment<TGameContext, TItemId>(itemResolver, availableSlots, data);
                return true;
            }

            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, in SlottedEquipment<TGameContext, TItemId> t, out TActorId changedK)
        {
            ref var data = ref t.Data;
            v.AssignOrReplace(k, in data);
            changedK = k;
            return true;
        }

        public bool TryRemove(IEntityViewControl<TActorId> entityRegistry, TGameContext context, TActorId k, out TActorId changedItem)
        {
            changedItem = k;
            return false;
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