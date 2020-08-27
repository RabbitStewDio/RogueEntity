using EnTTSharp.Entities;

namespace RogueEntity.Core.Meta.Items
{
    public abstract class SimpleItemComponentTraitBase<TGameContext, TItemId, TData> : IItemComponentTrait<TGameContext, TItemId, TData>,
                                                                                       IBulkItemTrait<TGameContext, TItemId>,
                                                                                       IReferenceItemTrait<TGameContext, TItemId>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        protected SimpleItemComponentTraitBase(string id, int priority)
        {
            Id = id;
            Priority = priority;
        }

        protected abstract TData CreateInitialValue(TGameContext c, TItemId reference);

        public string Id { get; }
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

        public virtual void Initialize(IEntityViewControl<TItemId> v,
                                       TGameContext context,
                                       TItemId k,
                                       IItemDeclaration item)
        {
            v.AssignOrReplace(k, CreateInitialValue(context, k));
        }

        public abstract void Apply(IEntityViewControl<TItemId> v,
                                   TGameContext context,
                                   TItemId k,
                                   IItemDeclaration item);

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out TData t)
        {
            if (k.IsReference)
            {
                if (v.IsValid(k) &&
                    v.GetComponent(k, out t))
                {
                    return true;
                }

                t = default;
                return false;
            }

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

            if (v.IsValid(k))
            {
                v.AssignOrReplace(k, in t);
                changedK = k;
                return true;
            }

            if (k.IsReference)
            {
                changedK = k;
                return false;
            }

            return TryUpdateBulkData(k, in t, out changedK);
        }

        public bool TryRemove(IEntityViewControl<TItemId> entityRegistry, TGameContext context, TItemId k, out TItemId changedItem)
        {
            if (!k.IsReference)
            {
                changedItem = k;
                return false;
            }

            entityRegistry.RemoveComponent<TData>(k);
            changedItem = k;
            return true;
        }

        protected virtual bool ValidateData(IEntityViewControl<TItemId> entityViewControl, TGameContext context,
                                            in TItemId itemReference, in TData data)
        {
            return true;
        }

        protected virtual bool TryUpdateBulkData(TItemId k, in TData data, out TItemId changedK)
        {
            changedK = k;
            return false;
        }
    }
}