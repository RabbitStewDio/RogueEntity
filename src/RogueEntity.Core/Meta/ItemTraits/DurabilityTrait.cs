﻿using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public class DurabilityTrait<TContext, TItemId> : IItemComponentTrait<TContext, TItemId, Durability>, 
                                                      IBulkDataTrait<TContext, TItemId>,
                                                      IReferenceItemTrait<TContext, TItemId> 
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        readonly Durability baseValue;

        public DurabilityTrait(ushort maxDurability): this(maxDurability, maxDurability)
        {
        }

        public DurabilityTrait(ushort initialCount, ushort maxDurability)
        {
            Id = "ItemTrait.Generic.Durability";
            Priority = 100;
            this.baseValue = new Durability(initialCount, maxDurability);
        }

        public string Id { get; }
        public int Priority { get; }

        public void Initialize(IEntityViewControl<TItemId> v, TContext context, TItemId k, IItemDeclaration item)
        {
            v.AssignOrReplace(k, in baseValue);
        }

        public void Apply(IEntityViewControl<TItemId> v, TContext context, TItemId k, IItemDeclaration item)
        {
        }

        public virtual TItemId Initialize(TContext context, IItemDeclaration item, TItemId reference)
        {
            return reference.WithData(baseValue.HitPoints);
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TContext context, TItemId k, out Durability t)
        {
            if (k.IsReference)
            {
                return v.GetComponent(k, out t);
            }

            t = baseValue.WithHitPoints(k.Data);
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TContext context, TItemId k, 
                              in Durability t, out TItemId changedK)
        {
            if (k.IsReference)
            {
                v.WriteBack(k, in t);
                changedK = k;
                return true;
            }

            changedK = k.WithData(t.HitPoints);
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> entityRegistry, TContext context, TItemId k, out TItemId changedItem)
        {
            changedItem = k;
            return false;
        }
    }
}