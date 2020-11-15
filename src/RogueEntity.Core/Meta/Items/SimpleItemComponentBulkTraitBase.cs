using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public abstract class SimpleItemComponentBulkTraitBase<TGameContext, TItemId, TData> : IItemComponentTrait<TGameContext, TItemId, TData>,
                                                                                           IBulkItemTrait<TGameContext, TItemId>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        protected SimpleItemComponentBulkTraitBase(ItemTraitId id, int priority)
        {
            Id = id;
            Priority = priority;
        }

        protected abstract TData CreateInitialValue(TGameContext c, TItemId reference);

        public ItemTraitId Id { get; }
        public int Priority { get; }

        public TItemId Initialize(TGameContext context, IItemDeclaration item, TItemId reference)
        {
            // bulk items have no component storage.
            if (TryUpdateBulkData(reference, CreateInitialValue(context, reference), out var k))
            {
                return k;
            }

            return reference;
        }

        public virtual IBulkItemTrait<TGameContext, TItemId> CreateInstance()
        {
            return this;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out TData t)
        {
            return TryQueryBulkData(v, context, k, out t);
        }

        protected virtual bool TryQueryBulkData(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out TData t)
        {
            t = CreateInitialValue(context, k);
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in TData t, out TItemId changedK)
        {
            if (!ValidateData(v, context, k, in t))
            {
                changedK = k;
                return false;
            }

            return TryUpdateBulkData(k, in t, out changedK);
        }

        public virtual bool TryRemove(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out TItemId changedItem)
        {
            changedItem = k;
            return false;
        }

        protected virtual bool ValidateData(IEntityViewControl<TItemId> v, TGameContext context,
                                            in TItemId itemReference, in TData data)
        {
            return true;
        }

        protected virtual bool TryUpdateBulkData(TItemId k, in TData data, out TItemId changedK)
        {
            changedK = k;
            return false;
        }

        public abstract IEnumerable<EntityRoleInstance> GetEntityRoles();

        public virtual IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }
    }
}