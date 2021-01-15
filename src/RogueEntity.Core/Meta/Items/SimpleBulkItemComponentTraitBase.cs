using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public abstract class SimpleBulkItemComponentTraitBase<TItemId, TData> : IItemComponentTrait<TItemId, TData>,
                                                                             IBulkItemTrait<TItemId>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        protected SimpleBulkItemComponentTraitBase(ItemTraitId id, int priority)
        {
            Id = id;
            Priority = priority;
        }

        protected abstract TData CreateInitialValue(TItemId reference);

        public ItemTraitId Id { get; }
        public int Priority { get; }

        public TItemId Initialize(IItemDeclaration item, TItemId reference)
        {
            // bulk items have no component storage.
            if (TryUpdateBulkData(reference, CreateInitialValue(reference), out var k))
            {
                return k;
            }

            return reference;
        }

        public virtual IBulkItemTrait<TItemId> CreateInstance()
        {
            return this;
        }

        public bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out TData t)
        {
            return TryQueryBulkData(v, k, out t);
        }

        protected virtual bool TryQueryBulkData(IEntityViewControl<TItemId> v, TItemId k, out TData t)
        {
            t = CreateInitialValue(k);
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TItemId k, in TData t, out TItemId changedK)
        {
            if (!ValidateData(v, k, in t))
            {
                changedK = k;
                return false;
            }

            return TryUpdateBulkData(k, in t, out changedK);
        }

        public virtual bool TryRemove(IEntityViewControl<TItemId> v, TItemId k, out TItemId changedItem)
        {
            changedItem = k;
            return false;
        }

        protected virtual bool ValidateData(IEntityViewControl<TItemId> v,
                                            in TItemId itemReference,
                                            in TData data)
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
