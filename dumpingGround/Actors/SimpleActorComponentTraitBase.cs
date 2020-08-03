using EnttSharp.Entities;

namespace ValionRL.Core.Infrastructure.Meta.Actors
{
    public abstract class SimpleActorComponentTraitBase<TGameContext, TActorId, TData> : IActorComponentTrait<TGameContext, TActorId, TData>
        where TActorId : IEntityKey
    {
        protected SimpleActorComponentTraitBase(string id, int priority)
        {
            Id = id;
            Priority = priority;
        }

        public string Id { get; }
        public int Priority { get; }

        protected abstract TData CreateInitialValue(TGameContext c, TActorId actor);

        public virtual void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k)
        {
            v.AssignOrReplace(k, CreateInitialValue(context, k));
        }

        public virtual void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k)
        {
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out TData t)
        {
            if (v.IsValid(k) &&
                v.GetComponent(k, out t))

            {
                return true;
            }

            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, in TData t)
        {
            if (!ValidateData(v, context, k, in t))
            {
                return false;
            }

            if (v.IsValid(k))
            {
                v.AssignOrReplace(k, in t);
                return true;
            }

            return false;
        }

        protected virtual bool ValidateData(IEntityViewControl<TActorId> entityViewControl, TGameContext context,
                                            in TActorId actorReference, in TData data)
        {
            return true;
        }

    }
}