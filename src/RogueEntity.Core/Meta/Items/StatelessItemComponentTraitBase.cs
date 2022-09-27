using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public abstract class StatelessItemComponentTraitBase<TItemId, TData> : IItemComponentInformationTrait<TItemId, TData>,
                                                                            IBulkItemTrait<TItemId>,
                                                                            IReferenceItemTrait<TItemId>
        where TItemId : struct, IEntityKey

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
