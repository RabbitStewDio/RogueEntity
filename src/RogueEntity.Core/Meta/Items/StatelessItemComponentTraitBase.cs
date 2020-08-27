using EnTTSharp.Entities;

namespace RogueEntity.Core.Meta.Items
{
    public abstract class StatelessItemComponentTraitBase<TGameContext, TItemId, TData> : IItemComponentTrait<TGameContext, TItemId, TData>,
                                                                                          IBulkItemTrait<TGameContext, TItemId>, 
                                                                                          IReferenceItemTrait<TGameContext, TItemId> 
        where TItemId : IEntityKey

    {
        protected StatelessItemComponentTraitBase(string id, int priority)
        {
            Id = id;
            Priority = priority;
        }

        public string Id { get; }
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
    }
}