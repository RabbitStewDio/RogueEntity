using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Meta.ItemTraits
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

        public DurabilityTrait(ushort maxDurability, ushort initialCount)
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
                t = baseValue;
                return true;
            }

            t = baseValue.WithHitPoints(k.Data);
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TContext context, TItemId k, 
                              in Durability t, out TItemId changedK)
        {
            if (k.IsReference)
            {
                changedK = k;
                return false;
            }

            changedK = k.WithData(t.HitPoints);
            return true;
        }
    }
}