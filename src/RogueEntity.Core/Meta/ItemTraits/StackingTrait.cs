using System;
using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public class StackingTrait<TContext, TItemId> : IItemComponentTrait<TContext, TItemId, StackCount>, 
                                                    IBulkDataTrait<TContext, TItemId> 
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        readonly ushort stackSize;
        readonly ushort initialCount;

        public StackingTrait(ushort stackSize): this(stackSize, stackSize)
        {
        }

        public StackingTrait(ushort initialCount, ushort stackSize)
        {
            Id = "ItemTrait.Bulk.Generic.Stacking";
            Priority = 100;
            this.stackSize = stackSize;
            this.initialCount = initialCount;
        }

        public ItemTraitId Id { get; }
        public int Priority { get; }

        public IBulkItemTrait<TContext, TItemId> CreateInstance()
        {
            return this;
        }

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

            t = StackCount.Of(stackSize).WithCount(k.Data);
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TContext context, TItemId k, in StackCount t, out TItemId changedK)
        {
            if (t.MaximumStackSize > stackSize || t.Count > t.MaximumStackSize)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (k.IsReference)
            {
                changedK = k;
                return false;
            }

            if (t.Count == 0)
            {
                changedK = default;
            }
            else
            {
                changedK = k.WithData(t.Count);
            }
            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> entityRegistry, TContext context, TItemId k, out TItemId changedItem)
        {
            changedItem = k;
            return false;
        }
        
        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return CoreModule.ItemRole.Instantiate<TItemId>();
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }
    }
}