using EnttSharp.Entities;

namespace ValionRL.Core.Infrastructure.Meta.Actors
{
    public interface IActorComponentTrait<TContext, TActorId, TData> : IActorTrait<TContext, TActorId> 
        where TActorId : IEntityKey
    {
        bool TryQuery(IEntityViewControl<TActorId> v, TContext context, TActorId k, out TData t);
        bool TryUpdate(IEntityViewControl<TActorId> v, TContext context, TActorId k, in TData t);
    }
}