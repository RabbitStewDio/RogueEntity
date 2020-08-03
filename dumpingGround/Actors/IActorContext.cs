using EnttSharp.Entities;

namespace ValionRL.Core.Infrastructure.Meta.Actors
{
    public interface IActorContext<TGameContext, TActorId> 
        where TActorId : IEntityKey
    {
        IActorResolver<TGameContext, TActorId> ActorResolver { get; }
    }
}