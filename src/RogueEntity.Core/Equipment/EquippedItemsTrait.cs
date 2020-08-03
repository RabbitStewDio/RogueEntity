using System.Collections.Generic;
using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Meta.Items;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Equipment
{
    public class EquippedItemsTrait<TGameContext, TItemId> : SimpleItemComponentTraitBase<TGameContext, TItemId, SlottedEquipment<TGameContext, TItemId>>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        readonly ReadOnlyListWrapper<EquipmentSlot> availableSlots;
        readonly IItemResolver<TGameContext, TItemId> itemResolver;

        public EquippedItemsTrait(IItemResolver<TGameContext, TItemId> itemResolver, 
                                  IEnumerable<EquipmentSlot> availableSlots) : base("Core.Actor.Equipment", 300)
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

        protected override SlottedEquipment<TGameContext, TItemId> CreateInitialValue(TGameContext c, TItemId actor)
        {
            return new SlottedEquipment<TGameContext, TItemId>(itemResolver, availableSlots, new SlottedEquipmentData<TItemId>());
        }

        public override void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
        }

        /* TODO
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