using EnttSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Meta.Items
{
    public abstract class StatelessItemComponentTraitBase<TGameContext, TItemId, TData> : IItemComponentTrait<TGameContext, TItemId, TData>,
                                                                                          IBulkItemTrait<TGameContext, TItemId>, IReferenceItemTrait<TGameContext, TItemId> 
        where TItemId : IBulkDataStorageKey<TItemId>

    {
        protected StatelessItemComponentTraitBase(string id, int priority)
        {
            Id = id;
            Priority = priority;
        }

        public string Id { get; }
        public int Priority { get; }

        protected abstract TData GetData(TGameContext context);

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
            t = GetData(context);
            return true;
        }

        public bool TryUpdate(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, in TData t, out TItemId changedK)
        {
            changedK = k;
            return false;
        }
    }
}