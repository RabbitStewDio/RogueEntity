using System;
using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public class StackingTrait<TContext, TItemId> : IItemComponentTrait<TContext, TItemId, StackCount>, 
                                                    IBulkDataTrait<TContext, TItemId> 
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        protected readonly ushort StackSize;
        readonly ushort initialCount;

        public StackingTrait(ushort stackSize): this(stackSize, stackSize)
        {
        }

        public StackingTrait(ushort stackSize, ushort initialCount)
        {
            Id = "ItemTrait.Bulk.Generic.Stacking";
            Priority = 100;
            this.StackSize = stackSize;
            this.initialCount = initialCount;
        }

        public string Id { get; }
        public int Priority { get; }

        public virtual TItemId Initialize(TContext context, IItemDeclaration item, TItemId reference)
        {
            return reference.WithData(initialCount);
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TContext context, TItemId k, out StackCount t)
        {
            if (k.IsReference)
            {
                t = StackCount.Of(1).WithCount(1);
                return true;
            }

            t = StackCount.Of(StackSize).WithCount(k.Data);
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TContext context, TItemId k, in StackCount t, out TItemId changedK)
        {
            if (t.MaximumStackSize > StackSize || t.Count > t.MaximumStackSize)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (k.IsReference)
            {
                changedK = k;
                return false;
            }

            changedK = k.WithData(t.Count);
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> entityRegistry, TContext context, TItemId k, out TItemId changedItem)
        {
            changedItem = k;
            return false;
        }
    }
}