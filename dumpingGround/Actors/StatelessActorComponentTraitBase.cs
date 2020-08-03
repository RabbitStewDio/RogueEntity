using EnttSharp.Entities;

namespace ValionRL.Core.Infrastructure.Meta.Actors
{
    public abstract class StatelessActorComponentTraitBase<TGameContext, TActorId, TData> : IActorComponentTrait<TGameContext, TActorId, TData> 
        where TActorId : IEntityKey
    {
        protected StatelessActorComponentTraitBase(string id, int priority)
        {
            Id = id;
            Priority = priority;
        }

        public string Id { get; }
        public int Priority { get; }

        protected abstract TData CreateInitialValue(TGameContext c);

        public virtual void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k)
        {
        }

        void IActorTrait<TGameContext, TActorId>.Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k)
        {
        }

        bool IActorComponentTrait<TGameContext, TActorId, TData>.TryUpdate(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, in TData t)
        {
            return false;
        }

        public virtual bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out TData t)
        {
            t = CreateInitialValue(context);
            return true;
        }
    }
}