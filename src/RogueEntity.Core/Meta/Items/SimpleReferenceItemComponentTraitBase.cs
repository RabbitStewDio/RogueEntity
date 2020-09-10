﻿using EnTTSharp.Entities;

namespace RogueEntity.Core.Meta.Items
{
    public abstract class SimpleReferenceItemComponentTraitBase<TGameContext, TItemId, TData>: IItemComponentTrait<TGameContext, TItemId, TData>,
                                                                                               IReferenceItemTrait<TGameContext, TItemId>
        where TItemId : IEntityKey
    {
        protected SimpleReferenceItemComponentTraitBase(string id, int priority)
        {
            Id = id;
            Priority = priority;
        }

        protected abstract TData CreateInitialValue(TGameContext c, TItemId reference);

        public string Id { get; }
        public int Priority { get; }

        public bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out TData t)
        {
            if (v.IsValid(k) &&
                v.GetComponent(k, out t))
            {
                return true;
            }

            t = default;
            return false;
        }

        public virtual void Initialize(IEntityViewControl<TItemId> v,
                                       TGameContext context,
                                       TItemId k,
                                       IItemDeclaration item)
        {
            v.AssignOrReplace(k, CreateInitialValue(context, k));
        }

        public virtual void Apply(IEntityViewControl<TItemId> v,
                                  TGameContext context,
                                  TItemId k,
                                  IItemDeclaration item)
        {
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

            changedK = k;
            return false;
        }

        public bool TryRemove(IEntityViewControl<TItemId> entityRegistry, TGameContext context, TItemId k, out TItemId changedItem)
        {
            entityRegistry.RemoveComponent<TData>(k);
            changedItem = k;
            return true;
        }

        protected virtual bool ValidateData(IEntityViewControl<TItemId> entityViewControl, TGameContext context,
                                            in TItemId itemReference, in TData data)
        {
            return true;
        }
    }
}