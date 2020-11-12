using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public abstract class StatelessItemComponentTraitBase<TGameContext, TItemId, TData> : IItemComponentTrait<TGameContext, TItemId, TData>,
                                                                                          IBulkItemTrait<TGameContext, TItemId>, 
                                                                                          IReferenceItemTrait<TGameContext, TItemId> 
        where TItemId : IEntityKey

    {
        protected StatelessItemComponentTraitBase(ItemTraitId id, int priority)
        {
            Id = id;
            Priority = priority;
        }

        public ItemTraitId Id { get; }
        public int Priority { get; }

        protected abstract TData GetData(TGameContext context, TItemId k);

        TItemId IBulkItemTrait<TGameContext, TItemId>.Initialize(TGameContext context, IItemDeclaration item, TItemId reference)
        {
            return reference;
        }

        void IReferenceItemTrait<TGameContext, TItemId>.Initialize(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
        }

        public virtual void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
        }

        public virtual bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out TData t)
        {
            t = GetData(context, k);
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in TData t, out TItemId changedK)
        {
            changedK = k;
            return false;
        }

        public bool TryRemove(IEntityViewControl<TItemId> entityRegistry, TGameContext context, TItemId k, out TItemId changedItem)
        {
            changedItem = k;
            return false;
        }

        protected virtual StatelessItemComponentTraitBase<TGameContext, TItemId, TData> CreateInstance()
        {
            return this;
        }

        IBulkItemTrait<TGameContext, TItemId> IBulkItemTrait<TGameContext, TItemId>.CreateInstance()
        {
            return CreateInstance();
        }

        IReferenceItemTrait<TGameContext, TItemId> IReferenceItemTrait<TGameContext, TItemId>.CreateInstance()
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