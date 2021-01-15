using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public abstract class StatelessItemComponentTraitBase<TItemId, TData> : IItemComponentTrait<TItemId, TData>,
                                                                            IBulkItemTrait<TItemId>,
                                                                            IReferenceItemTrait<TItemId>
        where TItemId : IEntityKey

    {
        protected StatelessItemComponentTraitBase(ItemTraitId id, int priority)
        {
            Id = id;
            Priority = priority;
        }

        public ItemTraitId Id { get; }
        public int Priority { get; }

        protected abstract TData GetData(TItemId k);

        TItemId IBulkItemTrait<TItemId>.Initialize(IItemDeclaration item, TItemId reference)
        {
            return reference;
        }

        void IReferenceItemTrait<TItemId>.Initialize(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
        { }

        public virtual void Apply(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
        { }

        public virtual bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out TData t)
        {
            t = GetData(k);
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TItemId k, in TData t, out TItemId changedK)
        {
            changedK = k;
            return false;
        }

        public bool TryRemove(IEntityViewControl<TItemId> entityRegistry, TItemId k, out TItemId changedItem)
        {
            changedItem = k;
            return false;
        }

        protected virtual StatelessItemComponentTraitBase<TItemId, TData> CreateInstance()
        {
            return this;
        }

        IBulkItemTrait<TItemId> IBulkItemTrait<TItemId>.CreateInstance()
        {
            return CreateInstance();
        }

        IReferenceItemTrait<TItemId> IReferenceItemTrait<TItemId>.CreateInstance()
        {
            return CreateInstance();
        }

        public abstract IEnumerable<EntityRoleInstance> GetEntityRoles();

        public virtual IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }
    }
}
