using System;
using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public class StackingBulkTrait<TItemId> : IItemComponentTrait<TItemId, StackCount>,
                                              IBulkDataTrait<TItemId>
        where TItemId : struct, IBulkDataStorageKey<TItemId>
    {
        readonly ushort stackSize;
        readonly ushort initialCount;

        public StackingBulkTrait(ushort stackSize) : this(stackSize, stackSize)
        {
        }

        public StackingBulkTrait(ushort initialCount, ushort stackSize)
        {
            if (stackSize <= 0 || stackSize >= ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(stackSize));
            }

            if (initialCount <= 0 || initialCount > stackSize)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCount));
            }

            Id = "ItemTrait.Bulk.Generic.Stacking";
            Priority = 100;
            this.stackSize = (ushort)(stackSize - 1);
            this.initialCount = (ushort)(initialCount - 1);
        }

        public ItemTraitId Id { get; }
        public int Priority { get; }

        public IBulkItemTrait<TItemId> CreateInstance()
        {
            return this;
        }

        public virtual TItemId Initialize(IItemDeclaration item, TItemId reference)
        {
            return reference.WithData(initialCount);
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out StackCount t)
        {
            if (k.IsReference)
            {
                t = StackCount.Of(1, 1);
                return true;
            }

            t = StackCount.OfRaw(k.Data.ClampToUnsignedShort(), stackSize);
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TItemId k, in StackCount t, out TItemId changedK)
        {
            if (t.MaximumStackSize != (stackSize + 1))
            {
                throw new ArgumentOutOfRangeException(nameof(t), $"StackCount {t} does not match expected maximum stack size {stackSize + 1}");
            }

            if (t.Count > t.MaximumStackSize ||
                t.Count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(t), $"Stack is invalid {t}");
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
                changedK = k.WithData(t.CountRaw);
            }

            return true;
        }

        public bool TryRemove(IEntityViewControl<TItemId> entityRegistry, TItemId k, out TItemId changedItem)
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